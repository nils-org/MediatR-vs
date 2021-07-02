using System;
using System.Windows;
using System.Windows.Input;

namespace MediatRvs
{
    // https://stackoverflow.com/a/4498006/180156

    public class MouseDoubleClick
    {
        private const int DefaultTimeoutMillis = 200;

        public static DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command",
                typeof(ICommand),
                typeof(MouseDoubleClick),
                new UIPropertyMetadata(CommandChanged));

        public static DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter",
                typeof(object),
                typeof(MouseDoubleClick),
                new UIPropertyMetadata(null));

        public static DependencyProperty DoubleClickTimeoutProperty =
            DependencyProperty.RegisterAttached("DoubleClickTimeout",
                typeof(TimeSpan),
                typeof(MouseDoubleClick),
                new UIPropertyMetadata(null));

        public static void SetCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(CommandProperty, value);
        }

        public static ICommand GetCommand(DependencyObject target)
        {
            return (ICommand)target.GetValue(CommandProperty);
        }

        public static void SetCommandParameter(DependencyObject target, object value)
        {
            target.SetValue(CommandParameterProperty, value);
        }

        public static object GetCommandParameter(DependencyObject target)
        {
            return target.GetValue(CommandParameterProperty);
        }

        public static void SetDoubleClickTimeout(DependencyObject target, TimeSpan value)
        {
            target.SetValue(DoubleClickTimeoutProperty, value);
        }

        public static TimeSpan GetDoubleClickTimeout(DependencyObject target)
        {
            var timeSpan = (TimeSpan)target.GetValue(DoubleClickTimeoutProperty);
            if (timeSpan == TimeSpan.Zero)
            {
                timeSpan = TimeSpan.FromMilliseconds(DefaultTimeoutMillis);
            }

            return timeSpan;
        }

        private static void CommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (!(target is FrameworkElement element))
            {
                throw new ArgumentException("Use MouseDoubleClick only on FrameworkElements!");
            }

            if (e.NewValue != null && e.OldValue == null)
            {
                element.MouseLeftButtonUp += OnMouseSingleClick;
            }
            else if (e.NewValue == null && e.OldValue != null)
            {
                element.MouseLeftButtonUp -= OnMouseSingleClick;
            }
        }

        private static object lastSender;
        private static DateTime lastClick;

        private static void OnMouseSingleClick(object sender, RoutedEventArgs e)
        {
            if (lastSender != null && ReferenceEquals(lastSender, sender))
            {
                var timeBetweenClicks = DateTime.Now - lastClick;
                var timeout = GetDoubleClickTimeout((DependencyObject)sender);

                if (timeBetweenClicks < timeout)
                {
                    // this is it!
                    lastSender = null;
                    OnMouseDoubleClick(sender, e);
                }

                return;
            }

            // this is the first click
            lastSender = sender;
            lastClick = DateTime.Now;
        }

        private static void OnMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            var target = (DependencyObject) sender;
            var command = GetCommand(target);
            var commandParameter = GetCommandParameter(target);
            command?.Execute(commandParameter);
        }
    }
}