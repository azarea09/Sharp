using Raylib_cs;

namespace SharpFramework.Scene
{
    /// <summary>
    /// シーンを管理するクラス。
    /// </summary>
    public class SceneManager
    {
        public SceneManager()
        {
            Scenes = new List<Scene>();
            CurrentScene = null;
        }

        /// <summary>
        /// 現在のシーンを切り替えます。
        /// </summary>
        /// <param name="scene">切り替えるシーン。</param>
        public void ChangeScene(Scene scene)
        {
            if (CurrentScene != null && CurrentScene != scene)
            {
                CurrentScene.Disable(); // 現在のシーンを無効化
                GC.Collect(); // ガベージコレクションを実行してメモリを解放
            }

            CurrentScene = scene;

            if (!Scenes.Contains(scene))
            {
                Scenes.Add(scene); // 未登録のシーンを追加
            }

            CurrentScene.Enable(); // 新しいシーンを有効化
        }

        /// <summary>
        /// シーンを追加します（有効化はしません）。
        /// </summary>
        public void AddScene(Scene scene)
        {
            if (!Scenes.Contains(scene))
            {
                Scenes.Add(scene);
            }
        }

        /// <summary>
        /// シーンを削除します。
        /// </summary>
        public void RemoveScene(Scene scene)
        {
            if (Scenes.Contains(scene))
            {
                if (CurrentScene == scene)
                {
                    CurrentScene = null; // 現在のシーンの場合はリセット
                }
                scene.Disable();
                Scenes.Remove(scene);
            }
        }

        /// <summary>
        /// すべてのシーンを削除します。
        /// </summary>
        public void DeleteAllScenes()
        {
            // 現在のシーンを無効化
            if (CurrentScene != null)
            {
                CurrentScene.Disable();
                CurrentScene = null;
            }

            // すべてのシーンを削除
            foreach (var scene in Scenes)
            {
                DeleteSceneRecursively(scene);
            }

            // シーンリストをクリア
            Scenes.Clear();
        }

        /// <summary>
        /// 再帰的にシーンを削除します。
        /// </summary>
        /// <param name="scene">削除するシーン。</param>
        private void DeleteSceneRecursively(Scene scene)
        {
            foreach (var child in scene.ChildScenes)
            {
                DeleteSceneRecursively(child);
            }

            scene.Disable(); // シーンを無効化
        }

        /// <summary>
        /// 描画します（現在のシーンのみ）。
        /// </summary>
        public void Draw()
        {
            if (CurrentScene == null) return;

            Raylib.ClearBackground(Color.Black); // 背景色をクリア

            CurrentScene.Draw(); // 現在のシーンを描画
        }

        /// <summary>
        /// 更新します（現在のシーンのみ）。
        /// </summary>
        public void Update()
        {
            CurrentScene?.Update();
        }

        public List<Scene> Scenes { get; private set; }
        public Scene CurrentScene { get; private set; }
    }
}
