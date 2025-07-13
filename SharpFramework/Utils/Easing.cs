namespace SharpFramework.Utils
{
    /// <summary>
    /// イージング処理を行う静的クラス。各種イージング関数や、指定時間内での値の補間を提供する。
    /// </summary>
    public static class Easing
    {
        /// <summary>イージングの種類を表す列挙体</summary>
        public enum CalcType
        {
            None,   // 線形
            Quad,   // 2次（Quadratic）
            Cubic,  // 3次
            Quart,  // 4次
            Quint,  // 5次
            Sine,   // Sinカーブ
            Expo,   // 指数
            Circ    // 円形
        }

        /// <summary>
        /// 値を0.0～1.0の範囲で受け取り、指定のイージングタイプで補間を計算して返します。
        /// </summary>
        public static double EaseT(double t, CalcType type, bool easeIn = true)
        {
            t = Math.Clamp(t, 0.0, 1.0);

            return (type, easeIn) switch
            {
                (CalcType.Quad, true) => t * t,
                (CalcType.Quad, false) => 1 - (1 - t) * (1 - t),

                (CalcType.Cubic, true) => t * t * t,
                (CalcType.Cubic, false) => 1 - Math.Pow(1 - t, 3),

                (CalcType.Quart, true) => t * t * t * t,
                (CalcType.Quart, false) => 1 - Math.Pow(1 - t, 4),

                (CalcType.Quint, true) => t * t * t * t * t,
                (CalcType.Quint, false) => 1 - Math.Pow(1 - t, 5),

                (CalcType.Sine, true) => 1 - Math.Cos(t * Math.PI / 2),
                (CalcType.Sine, false) => Math.Sin(t * Math.PI / 2),

                (CalcType.Expo, true) => t == 0 ? 0 : Math.Pow(2, 10 * (t - 1)),
                (CalcType.Expo, false) => t == 1 ? 1 : 1 - Math.Pow(2, -10 * t),

                (CalcType.Circ, true) => 1 - Math.Sqrt(1 - t * t),
                (CalcType.Circ, false) => Math.Sqrt(1 - Math.Pow(t - 1, 2)),

                _ => t // None or fallback: Linear
            };
        }

        /// <summary>
        /// 開始値から終了値へ、指定イージングのEaseInで補間した値を返します。
        /// </summary>
        /// <param name="counterValue">現在の経過フレーム</param>
        /// <param name="startPoint">開始値</param>
        /// <param name="endPoint">終了値</param>
        /// <param name="type">使用するイージングタイプ</param>
        /// <returns>補間後の値</returns>
        public static double EaseIn(double counterValue, double startPoint, double endPoint, CalcType type)
        {
            if (endPoint - startPoint <= 0) return endPoint;
            double t = Math.Clamp((counterValue - startPoint) / (endPoint - startPoint), 0.0, 1.0);
            return startPoint + (endPoint - startPoint) * EaseT(t, type, easeIn: true);
        }

        /// <summary>
        /// 開始値から終了値へ、指定イージングのEaseOutで補間した値を返します。
        /// </summary>
        /// <param name="counterValue">現在の経過フレーム</param>
        /// <param name="startPoint">開始値</param>
        /// <param name="endPoint">終了値</param>
        /// <param name="type">使用するイージングタイプ</param>
        /// <returns>補間後の値</returns>
        public static double EaseOut(double counterValue, double startPoint, double endPoint, CalcType type)
        {
            if (endPoint - startPoint <= 0) return endPoint;
            double t = Math.Clamp((counterValue - startPoint) / (endPoint - startPoint), 0.0, 1.0);
            return startPoint + (endPoint - startPoint) * EaseT(t, type, easeIn: false);
        }
    }
}
