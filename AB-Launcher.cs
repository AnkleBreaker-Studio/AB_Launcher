using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Markup;
using System.Xml;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Reflection;

[assembly: AssemblyTitle("AB Launcher")]
[assembly: AssemblyDescription("AnkleBreaker Studio - Quick Actions Hub")]
[assembly: AssemblyVersion("1.0.0.0")]

namespace ABLauncher
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string configPath = Path.Combine(exeDir, "actions.json");

            if (!File.Exists(configPath))
            {
                MessageBox.Show("actions.json not found in:\n" + exeDir, "AB Launcher - Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string json = File.ReadAllText(configPath);
            var serializer = new JavaScriptSerializer();
            var actions = serializer.Deserialize<List<Dictionary<string, object>>>(json);

            var app = new Application();
            var window = CreateWindow(actions, exeDir);
            app.Run(window);
        }

        static Window CreateWindow(List<Dictionary<string, object>> actions, string exeDir)
        {
            var window = new Window
            {
                Title = "AB Launcher",
                Width = 360,
                SizeToContent = SizeToContent.Height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = BrushFromHex("#1E1E2E"),
                Topmost = true
            };

            // Close on Escape
            window.KeyDown += (s, e) => { if (e.Key == Key.Escape) window.Close(); };

            var mainPanel = new StackPanel { Margin = new Thickness(16) };

            // Title
            mainPanel.Children.Add(new TextBlock
            {
                Text = "AB Launcher",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = BrushFromHex("#CBA6F7"),
                FontFamily = new FontFamily("Segoe UI"),
                Margin = new Thickness(0, 0, 0, 4)
            });

            // Subtitle
            mainPanel.Children.Add(new TextBlock
            {
                Text = "AnkleBreaker Studio - Quick Actions",
                FontSize = 11,
                Foreground = BrushFromHex("#6C7086"),
                FontFamily = new FontFamily("Segoe UI"),
                Margin = new Thickness(0, 0, 0, 16)
            });

            // Status text (at bottom)
            var statusText = new TextBlock
            {
                Text = "",
                FontSize = 11,
                Foreground = BrushFromHex("#A6E3A1"),
                FontFamily = new FontFamily("Segoe UI"),
                Margin = new Thickness(0, 10, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            // Buttons
            foreach (var action in actions)
            {
                string name = GetStr(action, "name");
                string icon = GetStr(action, "icon");
                string command = GetStr(action, "command");
                string type = GetStr(action, "type");
                string description = GetStr(action, "description");
                bool admin = action.ContainsKey("admin") && action["admin"] is bool && (bool)action["admin"];

                var btn = CreateButton(name, icon, description);

                // Capture for closure
                string _name = name, _command = command, _type = type;
                bool _admin = admin;
                string _exeDir = exeDir;

                btn.Click += (s, e) =>
                {
                    statusText.Text = "Running: " + _name + "...";
                    statusText.Foreground = BrushFromHex("#F9E2AF");

                    try
                    {
                        RunAction(_type, _command, _admin, _exeDir);
                        statusText.Text = _name + " - Launched!";
                        statusText.Foreground = BrushFromHex("#A6E3A1");
                    }
                    catch (Exception ex)
                    {
                        statusText.Text = "Error: " + ex.Message;
                        statusText.Foreground = BrushFromHex("#F38BA8");
                    }
                };

                mainPanel.Children.Add(btn);
            }

            mainPanel.Children.Add(statusText);
            window.Content = mainPanel;
            return window;
        }

        static Button CreateButton(string name, string icon, string description)
        {
            var btn = new Button
            {
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 0, 0, 6),
                Padding = new Thickness(16, 12, 16, 12),
                BorderThickness = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Background = BrushFromHex("#313244"),
                Foreground = BrushFromHex("#CDD6F4")
            };

            // Rounded border via template
            var template = new ControlTemplate(typeof(Button));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            borderFactory.SetValue(Border.PaddingProperty, new Thickness(16, 12, 16, 12));
            borderFactory.SetValue(Border.BackgroundProperty, BrushFromHex("#313244"));
            borderFactory.Name = "border";

            var contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            contentFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(contentFactory);
            template.VisualTree = borderFactory;

            // Hover trigger
            var hoverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty, BrushFromHex("#45475A"), "border"));
            template.Triggers.Add(hoverTrigger);

            // Press trigger
            var pressTrigger = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressTrigger.Setters.Add(new Setter(Border.BackgroundProperty, BrushFromHex("#585B70"), "border"));
            template.Triggers.Add(pressTrigger);

            btn.Template = template;

            // Content layout
            var sp = new StackPanel { Orientation = Orientation.Horizontal };

            sp.Children.Add(new TextBlock
            {
                Text = icon,
                FontSize = 18,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            });

            var textStack = new StackPanel();
            textStack.Children.Add(new TextBlock
            {
                Text = name,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = BrushFromHex("#CDD6F4")
            });

            if (!string.IsNullOrEmpty(description))
            {
                textStack.Children.Add(new TextBlock
                {
                    Text = description,
                    FontSize = 11,
                    Foreground = BrushFromHex("#6C7086")
                });
            }

            sp.Children.Add(textStack);
            btn.Content = sp;
            return btn;
        }

        static void RunAction(string type, string command, bool admin, string exeDir)
        {
            var psi = new ProcessStartInfo();

            switch (type)
            {
                case "bat":
                    psi.FileName = Path.Combine(exeDir, command);
                    if (admin) psi.Verb = "runas";
                    psi.UseShellExecute = true;
                    break;
                case "shell":
                    psi.FileName = "explorer.exe";
                    psi.Arguments = command;
                    psi.UseShellExecute = true;
                    break;
                case "exe":
                    psi.FileName = command;
                    if (admin) psi.Verb = "runas";
                    psi.UseShellExecute = true;
                    break;
                case "ps1":
                    psi.FileName = "powershell.exe";
                    string ps1Path = Path.Combine(exeDir, command);
                    psi.Arguments = "-NoProfile -ExecutionPolicy Bypass -File \"" + ps1Path + "\"";
                    if (admin) psi.Verb = "runas";
                    psi.UseShellExecute = true;
                    break;
                case "url":
                    psi.FileName = command;
                    psi.UseShellExecute = true;
                    break;
                default:
                    psi.FileName = command;
                    psi.UseShellExecute = true;
                    break;
            }

            Process.Start(psi);
        }

        static SolidColorBrush BrushFromHex(string hex)
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        static string GetStr(Dictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key) && dict[key] != null)
                return dict[key].ToString();
            return "";
        }
    }
}
