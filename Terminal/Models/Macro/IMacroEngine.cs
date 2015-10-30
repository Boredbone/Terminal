using System.Threading.Tasks;

namespace Terminal.Models.Macro
{
    public interface IMacroEngine
    {
        /// <summary>
        /// タイムアウト時間，0の場合タイムアウトなし
        /// </summary>
        int Timeout { get; set; }


        /// <summary>
        /// 指定時間待機
        /// </summary>
        /// <param name="timeMillisec"></param>
        /// <returns></returns>
        Task DelayAsync(int timeMillisec);

        /// <summary>
        /// 画面に文字列を表示
        /// </summary>
        /// <param name="text"></param>
        void Display(string text);

        /// <summary>
        /// 文字列を送信
        /// </summary>
        /// <param name="text"></param>
        Task SendAsync(string text);

        /// <summary>
        /// 文字列を送信
        /// </summary>
        /// <param name="text"></param>
        /// <param name="immediately"></param>
        Task SendAsync(string text, bool immediately);

        /// <summary>
        /// 何らかの返信が来るまで待つ
        /// </summary>
        /// <returns></returns>
        Task<string> WaitAsync();

        /// <summary>
        /// 指定文字列のいずれかが来るまで待つ
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        Task<int> WaitAsync(params string[] keywords);

        /// <summary>
        /// 返信が一行来るまで待つ
        /// </summary>
        /// <returns></returns>
        Task<string> WaitLineAsync();

        /// <summary>
        /// 返信が指定行数来るまで待つ
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<string[]> WaitLineAsync(int count);
    }
}