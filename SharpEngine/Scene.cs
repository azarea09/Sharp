using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpEngine
{
    /// <summary>
    /// シーンクラス。
    /// </summary>
    public class Scene
    {
        /// <summary>
        /// 親シーンへの参照。
        /// </summary>
        public Scene Parent { get; private set; }

        /// <summary>
        /// シーンの初期化を行います。
        /// </summary>
        public Scene()
        {
            Enabled = true;
            ChildScenes = new List<Scene>();
        }

        ~Scene()
        {
            Enabled = false;
            ChildScenes.Clear();
        }

        /// <summary>
        /// 子シーンを追加します。
        /// </summary>
        /// <param name="scene">子シーン。</param>
        /// <param name="index">挿入するインデックス。</param>
        public void AddChildScene(Scene scene, int index = -1)
        {
            scene.Parent = this; // 親シーンの参照を持たせておく
            if (index < 0 || index >= ChildScenes.Count)
            {
                ChildScenes.Add(scene);
            }
            else
            {
                ChildScenes.Insert(index, scene); // 指定された位置に挿入
            }
        }

        /// <summary>
        /// アクティブ化する。
        /// </summary>
        public virtual void Enable()
        {
            if (!Enabled) return;
            foreach (var item in ChildScenes)
            {
                item.Enable();
            }
        }

        /// <summary>
        /// 非アクティブ化する。
        /// </summary>
        public virtual void Disable()
        {
            if (!Enabled) return;
            foreach (var item in ChildScenes)
            {
                item.Disable();
            }
            ChildScenes.Clear();
        }

        /// <summary>
        /// 更新を行う。
        /// </summary>
        public virtual void Update()
        {
            if (!Enabled) return;

            // Create a copy of the collection to avoid modification during enumeration
            var childScenesCopy = new List<Scene>(ChildScenes);

            foreach (var item in childScenesCopy)
            {
                item.Update();
            }
        }

        /// <summary>
        /// 描画を行う。
        /// </summary>
        public virtual void Draw()
        {
            if (!Enabled) return;

            foreach (var item in ChildScenes)
            {
                item.Draw();
            }
        }

        /// <summary>
        /// そのシーンの名前(名前空間付き)を返します。
        /// </summary>
        /// <returns>そのシーンの名前(名前空間付き)。</returns>
        public override string ToString()
        {
            return GetType().ToString();
        }

        /// <summary>
        /// 利用可能かどうか。
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// 子シーン。
        /// </summary>
        public List<Scene> ChildScenes { get; private set; }
    }
}
