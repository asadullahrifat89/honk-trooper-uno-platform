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

        #region Tree

        public bool GenerateTreeTop()
        {
            Construct tree = GenerateTree();
            tree.SetPosition(left: (tree.Width / 3 * _scene.Scaling) * 9, top: tree.Height * -1);

            Console.WriteLine("Tree generated.");

            return true;
        }

        public bool GenerateTreeBottom()
        {
            Construct tree = GenerateTree();
            tree.SetPosition(left: -1 * tree.Width * _scene.Scaling, top: (_scene.Height / 4 * _scene.Scaling) * 2);

            Console.WriteLine("Tree generated.");

            return true;
        }

        private Construct GenerateTree()
        {
            var size = Constants.CONSTRUCT_SIZES.FirstOrDefault(x => x.ConstructType == ConstructType.TREE);

            Construct tree = new(
                   speed: 3,
                   constructType: ConstructType.TREE,
                   width: size.Width * _scene.Scaling,
                   height: size.Height * _scene.Scaling,
                   animateAction: AnimateTree,
                   recycleAction: RecycleTree,
                   content: new Image()
                   {
                       Source = new BitmapImage(uriSource: Constants.CONSTRUCT_TEMPLATES.FirstOrDefault(x => x.ConstructType == ConstructType.TREE).Uri)
                   });

            _scene.AddToScene(tree);
            return tree;
        }


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

            //if (hitBox.Top > _scene.Height || hitBox.Left > _scene.Width)
            //{
            //    tree.SetPosition(left: -300, top: _scene.Height / 2);
            //}

            if (hitBox.Top > _scene.Height || hitBox.Left > _scene.Width)
                _scene.DisposeFromScene(tree);
            return true;
        }

        #endregion

        #endregion

        #region Events

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += MainPage_SizeChanged;
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            //var _windowWidth = args.NewSize.Width;
            //var _windowHeight = args.NewSize.Height;

            //_scene.Width = _windowWidth;
            //_scene.Height = _windowHeight;

            _scene.Width = 1920;
            _scene.Height = 1080;


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
            Generator treeGenBottom = new(generationDelay: 100, generationAction: GenerateTreeBottom);
            Generator treeGenTop = new(generationDelay: 100, generationAction: GenerateTreeTop);

            _scene.AddToScene(treeGenBottom);
            _scene.AddToScene(treeGenTop);

            _scene.Start();
        }

        #endregion
    }
}
