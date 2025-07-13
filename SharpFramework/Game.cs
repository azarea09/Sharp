namespace SharpFramework
{
    /// <summary>
    /// ゲームの初期化、描画更新を行うクラス。
    /// </summary>
    public abstract class Game
    {
        public abstract void Init();
        public abstract void Update();
        public abstract void Draw();
        public void Run()
        {
            Init();
            RaylibWrapper.Init();

            while (!RaylibWrapper.ShouldClose())
            {
                RaylibWrapper.Update();
                Update();
                RaylibWrapper.Draw(Draw);
            }

            RaylibWrapper.End();
        }
    }
}
