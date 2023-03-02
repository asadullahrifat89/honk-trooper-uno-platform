using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonkPooper
{
    public partial class Tree : Construct
    {
        private readonly Image _content = new() { Stretch = Stretch.Uniform };

        public Tree(double speed)
        {
            Tag = ConstructType.TREE;

            Speed = speed;

            SetSize(width: Constants.TREE_SIZE, height: Constants.TREE_SIZE);

            SetChild(_content);

            SetContent(Constants.ELEMENT_TEMPLATES.FirstOrDefault(x => x.ElementType == ConstructType.TREE).Uri);

            SetAction(
                movementAction: MovementAction,
                recycleAction: RecycleAction,
                destructionRule: DestructionRule.ExitsRightBorder,
                destructionImpact: DestructionImpact.Recycle);
        }

        public double Speed { get; set; }

        private bool MovementAction()
        {
            SetLeft(GetLeft() + Speed);

            Console.WriteLine($"X:{GetLeft()} Y:{GetTop()}");

            return true;
        }

        private bool RecycleAction()
        {
            SetLeft(0);

            Console.WriteLine($"Recycled");

            return true;
        }

        public void SetContent(Uri uri)
        {
            _content.Source = new BitmapImage(uri);            
        }
    }
}
