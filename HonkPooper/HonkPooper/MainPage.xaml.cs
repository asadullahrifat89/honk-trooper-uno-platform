using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace HonkPooper
{
    public sealed partial class MainPage : Page
    {
        private Scene _scene;

        #region Ctor

        public MainPage()
        {
            this.InitializeComponent();

            _scene = this.MainScene;
            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;
        }

        #endregion

        #region Methods

        public bool AnimateTree(Construct tree)
        {
            tree.SetLeft(tree.GetLeft() + tree.Speed);

            if (tree.GetLeft() + tree.Width > 0)
                tree.SetTop(tree.GetTop() + tree.Speed * 0.5);

            return true;
        }

        public bool RecycleTree(Construct tree)
        {
            var hitBox = tree.GetHitBox();

            if (hitBox.Top > _scene.Height || hitBox.Left > _scene.Width)
            {
                tree.SetPosition(left: -300, top: _scene.Height / 2);
            }

            return true;
        }

        #endregion

        #region Events

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += MainPage_SizeChanged;
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            var _windowWidth = args.NewSize.Width;
            var _windowHeight = args.NewSize.Height;

            _scene.Width = _windowWidth;
            _scene.Height = _windowHeight;
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= MainPage_SizeChanged;
        }

        private void InputView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

        }

        private void InputView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            for (int i = -4; i < 0; i++)
            {
                Construct tree = new(
                    speed: 2,
                    constructType: ConstructType.TREE,
                    width: Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.TREE).Width,
                    height: Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.TREE).Height,
                    animateAction: AnimateTree,
                    recycleAction: RecycleTree,
                    content: new Image()
                    {
                        Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.TREE).Uri)
                    });

                _scene.AddToScene(tree);
                tree.SetPosition(left: i * 300 * _scene.Scaling, top: _scene.Height / 2);
            }

            _scene.Animate();
        }

        #endregion
    }
}
