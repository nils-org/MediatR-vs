
#tool dotnet:?package=GitVersion.Tool&version=5.6.10
#addin nuget:?package=System.Net.Http&version=4.3.4


var configuration = Argument("configuration", "Release");
var target = Argument("target", "Default");

var artifacts = "./BuildArtifacts/";
var solutionPath = File("./src/MediatR-vs.sln");
var version = "0.0.0";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    if(!BuildSystem.IsLocalBuild) {
      // get version and update dll info
      var gitVersion = GitVersion(new GitVersionSettings {
          UpdateAssemblyInfo = true
      });

      // update vsix info
      var vsixs = GetFiles("./src/**/*.vsixmanifest");
      foreach (var vsix in vsixs)
      {
        Verbose($"Updating version of {vsix.FullPath}");
        XmlPoke(
          vsix, 
          "/x:PackageManifest/x:Metadata/x:Identity/@Version", 
          gitVersion.SemVer, 
          new XmlPokeSettings 
          {
            Namespaces = new Dictionary<string, string> 
            {
              { "x", "http://schemas.microsoft.com/developer/vsx-schema/2011" }
            }
          }
        );
      }
      
      version = gitVersion.SemVer;
      Information($"Building version {gitVersion.SemVer}.");
  }
  else
  {
      Information("Local build.");
      var vsix = GetFiles("./src/**/*.vsixmanifest").First();
      version = XmlPeek(
          vsix, 
          "/x:PackageManifest/x:Metadata/x:Identity/@Version", 
          new XmlPeekSettings 
          {
            Namespaces = new Dictionary<string, string> 
            {
              { "x", "http://schemas.microsoft.com/developer/vsx-schema/2011" }
            }
          });
  }
});

Teardown(ctx =>
{
  // Executed AFTER the last task.
  Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories("./src/**/bin/");
    CleanDirectories("./src/**/obj/");
    CleanDirectory(artifacts);
});

Task("Restore")
    .Does(() =>
{
    NuGetRestore(solutionPath);
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
    MSBuild(solutionPath, settings =>
        settings.SetPlatformTarget(PlatformTarget.MSIL)
            .SetMSBuildPlatform(MSBuildPlatform.x86)
            .UseToolVersion(MSBuildToolVersion.VS2019)
            .SetVerbosity(Verbosity.Quiet)
            .WithTarget("Build")
            .SetConfiguration(configuration));

    var files = GetFiles("./src/**/*.vsix");
    foreach (var file in files)
    {
      var target = artifacts + File($"{file.GetFilenameWithoutExtension().ToString()}-v{version}.vsix");
      CopyFile(file, target);
    }
});

Task("PublishToOpenGallery")
    .IsDependentOn("Build")
    .WithCriteria(() => !BuildSystem.IsLocalBuild)
    .Does(() =>
{
  var baseUrl = "https://www.vsixgallery.com/api/upload";
  var vsixs = GetFiles(artifacts + File("**/*.vsix"));
  var repo = System.Net.WebUtility.UrlEncode($"https://github.com/nils-org/MediatR-vs/");
  var issues = System.Net.WebUtility.UrlEncode($"https://github.com/nils-org/MediatR-vs/issues/");
  var client = new System.Net.Http.HttpClient();

  foreach (var vsix in vsixs)
  {
      var url = $"{baseUrl}?repo={repo}&issuetracker={issues}";
      var fileName = vsix.GetFilename().ToString();
      var content = new System.Net.Http.MultipartFormDataContent();
      var fileContent = new System.Net.Http.ByteArrayContent(
        System.IO.File.ReadAllBytes(
          MakeAbsolute(vsix).FullPath));
      fileContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("multipart/form-data");
      content.Add(fileContent, "file", fileName);
      var request = new System.Net.Http.HttpRequestMessage(
        System.Net.Http.HttpMethod.Post,
        url)
      {
        Content = content  
      };

      var response = client.Send(request);
      if(response.IsSuccessStatusCode) 
      {
        Information($"Uploaded {fileName}");
      }
      else
      {
          Warning($"Error uploading vsix. Status:{response.StatusCode} - {response.ReasonPhrase}");
          Verbose(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
      }
      
  }
});

Task("Default")
  .IsDependentOn("Build")
  .IsDependentOn("PublishToOpenGallery");

RunTarget(target);