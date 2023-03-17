namespace HonkTrooper.Wasm
{
    public sealed class Program
    {
        private static App _app;

        static int Main(string[] args)
        {
            Uno.UI.Xaml.Media.FontFamilyHelper.PreloadAsync("ms-appx:///HonkTrooper/Assets/Fonts/Gameplay.ttf#Gameplay");

            Microsoft.UI.Xaml.Application.Start(_ => _app = new AppHead());

            return 0;
        }
    }
}
