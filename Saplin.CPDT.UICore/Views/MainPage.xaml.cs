﻿using Saplin.CPDT.UICore.Animations;
using Saplin.CPDT.UICore.Controls;
using Saplin.CPDT.UICore.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Saplin.CPDT.UICore
{
    public partial class MainPage : ContentPage
    {
        private int narrowWidth = 640;
        private bool alreadyShown = false;

        Title title;
        SimpleUI simpleUI, simpleUIHeader;
        AdvancedUI advancedUI;
        TestInProgress testInProgress;
        TestCompletion testCompletion;
        TestSessionsPlaceholder testSessionsPlaceholder;
        Status status;
        Popups popups;
        OnlineDb onlineDb;

        private Task createMinimalUiTask, createRestOfUiTask;

        public MainPage()
        {
            //ASYNC AND DEFFERED creation of controls for better start-up time

            createMinimalUiTask = Task.Run(() =>
            {
                ApplyTheme();

                if (ViewModelContainer.NavigationViewModel.IsSimpleUI)
                {
                    simpleUI = new SimpleUI();
                    simpleUI.AdjustToWidth(Width);
                }
                else
                {
                    advancedUI = new AdvancedUI();
                    status = new Status();
                }
            });

            InitializeComponent();
            // Do not touch this page's control until InitilizeComponent is done
            title = new Title();
            title.QuitClicked += OnQuit;

            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);

            createMinimalUiTask.Wait();

            this.BackgroundColor = backgroundColor;

            createRestOfUiTask = Task.Run(() =>
            {
                simpleUIHeader = new SimpleUI();

                simpleUIHeader.SetBinding(IsVisibleProperty, new Binding("IsSimpleUIHeaderVisible", source: ViewModelContainer.NavigationViewModel));
                AbsoluteLayout.SetLayoutFlags(simpleUIHeader, AbsoluteLayoutFlags.None);
                simpleUIHeader.HorizontalOptions = LayoutOptions.CenterAndExpand;

                testInProgress = new TestInProgress();
                testCompletion = new TestCompletion();
                testSessionsPlaceholder = new TestSessionsPlaceholder();
                popups = new Popups();

                onlineDb = new OnlineDb();
 
                if (ViewModelContainer.NavigationViewModel.IsSimpleUI)
                {
                    advancedUI = new AdvancedUI();
                    status = new Status();
                }
                else
                {
                    simpleUI = new SimpleUI();
                }
            });

            stackLayout.Children.Add(title);
            if (ViewModelContainer.NavigationViewModel.IsSimpleUI)
            {
                absoluteLayout.Children.Add(simpleUI);
            }
            else
            {
                stackLayout.Children.Add(advancedUI);
                stackLayout.Children.Add(status);
            }

            // DISPLAYING CONTROLS HERE
            SizeChanged += (s, e) =>
            {
                if (alreadyShown)
                    AdaptLayoytToScreenWidth();
                else
                {
                    alreadyShown = true;

                    createRestOfUiTask.Wait();
                    AdaptLayoytToScreenWidth();

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (ViewModelContainer.NavigationViewModel.IsSimpleUI)
                        {
                            stackLayout.Children.Add(simpleUIHeader);
                            stackLayout.Children.Add(advancedUI);
                        }
                        else
                        {
                            absoluteLayout.Children.Add(simpleUI);
                            stackLayout.Children.Remove(status);
                            stackLayout.Children.Add(simpleUIHeader);
                        }

                        stackLayout.Children.Add(testInProgress);
                        stackLayout.Children.Add(testSessionsPlaceholder);
                        stackLayout.Children.Add(status);
                        absoluteLayout.Children.Add(popups);
                        absoluteLayout.Children.Add(testCompletion);

                        // From time to time devices don't have webview installed or properly configured, this may lead to app crash
                        try
                        {
                            foreach (var c in onlineDb.Children.ToArray())
                            {
                                absoluteLayout.Children.Add(c);
                            }
                        }
                        catch { advancedUI.MakeDbButtonRedirecting(); };

                        var now = DateTime.Now;
                        if ((now.Month == 12 && now.Day >= 20) || (now.Month == 1 && now.Day <= 10))
                        {
                            var lbl = new Label() { Text = "🎄", FontSize = 26 };
                            lbl.SetBinding(IsVisibleProperty, new Binding("IsSimpleUIStartPageVisible", source: ViewModelContainer.NavigationViewModel));
                            AbsoluteLayout.SetLayoutBounds(lbl, new Rectangle(0.5, 0.75, 40, 40));
                            AbsoluteLayout.SetLayoutFlags(lbl, AbsoluteLayoutFlags.PositionProportional);
                            absoluteLayout.Children.Add(lbl);
                        }
                    });

                }
            };
        }

        private Color backgroundColor = Color.Black;

        private void ApplyTheme()
        {
            if ((Xamarin.Forms.Application.Current as App).WhiteTheme)
            {
                var whiteTheme = new WhiteTheme();

                foreach (var key in whiteTheme.Keys)
                {
                    if (Xamarin.Forms.Application.Current.Resources.ContainsKey(key))
                        Xamarin.Forms.Application.Current.Resources[key] = whiteTheme[key];
                }

                backgroundColor = Color.White;
                //backgroundColor = Color.LightPink;
            }
        }

        private bool? alreadyNarrow;

        private void AdaptLayoytToScreenWidth()
        {
            simpleUI.AdjustToWidth(Width); // this control has different narrow threashold
            simpleUIHeader.AdjustToWidth(Width); //iOS and Android always have -1 in page constructor (rather than actual size as in Mac/?WPF)

            var narrow = Width < narrowWidth;

            if (alreadyNarrow.HasValue)
            {
                if (alreadyNarrow.Value && narrow) return;
                if (!alreadyNarrow.Value && !narrow) return;
            }

            alreadyNarrow = narrow;

            simpleUI?.AdjustToWidth(Width);
            simpleUIHeader?.AdjustToWidth(Width);
            advancedUI?.AdaptLayoytToScreenWidth(narrow);
            testInProgress?.AdaptLayoytToScreenWidth(narrow);

            testSessionsPlaceholder?.AdaptLayoytToScreenWidth(narrow);
            MasterDetail.AsyncPreloadDetailsForSelectionGroup("testSessions");

            ViewModelContainer.NavigationViewModel.IsNarrowView = narrow;

            AdjustPopupsToWidth(narrow);
        }

        private static void AdjustPopupsToWidth(bool narrow)
        {
            Xamarin.Forms.Application.Current.Resources["popUpContainer"] = narrow ?
                Xamarin.Forms.Application.Current.Resources["popUpContainerNarrow"]
                : Xamarin.Forms.Application.Current.Resources["popUpContainerWide"];
        }

        public void OnQuit(Object sender, EventArgs e)
        {
            CloseAplication();
        }

        public void CloseAplication()
        {
            title.QuitButton.IsVisible = false;
            title.QuitingMessage.IsVisible = true;
            ViewModelContainer.DriveTestViewModel.BreakTest.Execute(null);
            AnimationBase.DisposeAllAnimations(); // dispose off all animation controls in order to avoid unhandled exception on app close in WPF (if any animation is running on close) 
            if (ViewModelContainer.DriveTestViewModel.TestStarted)
            {

                Task.Run(() =>
                    {
                        var wait = new SpinWait();

                        while (ViewModelContainer.DriveTestViewModel.TestStarted) wait.SpinOnce();
                    }
                ).ContinueWith((t) =>
                    {
                        Thread.Sleep(1500);  // if there's any animation in-progress, give it time to complete
                        Device.BeginInvokeOnMainThread(() => { Xamarin.Forms.Application.Current.Quit(); }); ;
                    }
                );
            }
            else
            {
                Xamarin.Forms.Application.Current.Quit();
            }
        }

        bool clickedfOnceBeforeMinimization = false;

        protected override bool OnBackButtonPressed()
        {
            if (ViewModelContainer.NavigationViewModel.IsHomePage)
            {
                if (!ViewModelContainer.NavigationViewModel.IsSimpleUI)
                {
                    ViewModelContainer.NavigationViewModel.IsSimpleUI = true;
                }
                else if (!clickedfOnceBeforeMinimization)
                {
                    clickedfOnceBeforeMinimization = true;
                }
                else
                {
                    clickedfOnceBeforeMinimization = false;

                    var ph = DependencyService.Get<IPlatformHooks>();

                    try { ph?.MinimizeApp(); } catch { }
                }
            }

            if (ViewModelContainer.ResultsDbViewModel.IsVisible) ViewModelContainer.ResultsDbViewModel.IsVisible = false;
            else OnKeyPressed((char)27, SysKeys.Esc);

            return true;
        }

        // TODO - change to events
        public void OnKeyPressed(char key, SysKeys? sysKey)
        {
            if (!ViewModelContainer.ResultsDbViewModel.IsVisible)
            {
                if (key == 'q' || key == 'в')
                {
                    CloseAplication();
                }
                else if (sysKey != null)
                {
                    KeyPress.FindAndExecuteCommand(sysKey.Value);
                }
                else
                {
                    KeyPress.FindAndExecuteCommand(key);

                }

                BlinkingCursor.AddBlinkKey(key, sysKey);
            }
        }
    }
}