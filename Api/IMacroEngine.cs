using System;
using System.Threading.Tasks;

namespace Terminal.Macro.Api
{
    /// <summary>
    /// マクロの通信関連機能
    /// </summary>
    public interface IMacroEngine
    {
        /// <summary>
        /// タイムアウト時間(ミリ秒)，0の場合タイムアウトなし
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// 一行受信
        /// </summary>
        IObservable<string> LineReceived { get; }

        /// <summary>
        /// キャンセル状態の解除
        /// </summary>
        void ClearCancellation();

        /// <summary>
        /// 指定時間待機
        /// </summary>
        /// <param name="timeMillisec">待機時間(ミリ秒)</param>
        /// <returns>タスク</returns>
        /// <exception cref="System.OperationCanceledException">ユーザー操作によるマクロ実行のキャンセル</exception>
        Task DelayAsync(int timeMillisec);

        /// <summary>
        /// 画面に文字列を表示
        /// </summary>
        /// <param name="text">表示する文字列</param>
        void Display(string text);

        /// <summary>
        /// 受信履歴
        /// </summary>
        /// <param name="back">さかのぼる行数</param>
        /// <returns>指定された行の受信文字列</returns>
        string History(int back);

        /// <summary>
        /// 文字列を送信
        /// </summary>
        /// <param name="text">送信する文字列</param>
        /// <returns>タスク</returns>
        /// <exception cref="System.OperationCanceledException">ユーザー操作によるマクロ実行のキャンセル</exception>
        Task SendAsync(string text);

        /// <summary>
        /// 文字列を送信
        /// </summary>
        /// <param name="text">送信する文字列</param>
        /// <param name="immediately">待機トリガの設定を行わず，すぐに送信を開始する場合はtrue</param>
        /// <returns>タスク</returns>
        /// <exception cref="System.OperationCanceledException">ユーザー操作によるマクロ実行のキャンセル</exception>
        Task SendAsync(string text, bool immediately);

        /// <summary>
        /// ログの追尾をセット
        /// </summary>
        /// <param name="state"></param>
        void SetLogState(bool state);

        /// <summary>
        /// 何らかの返信が来るまで待つ
        /// </summary>
        /// <returns>受信した文字列</returns>
        /// <exception cref="System.TimeoutException">指定時間内に条件が達成されなかった</exception>
        /// <exception cref="System.OperationCanceledException">ユーザー操作によるマクロ実行のキャンセル</exception>
        Task<string> WaitAsync();

        /// <summary>
        /// 指定文字列のいずれかが来るまで待つ
        /// </summary>
        /// <param name="keywords">待機するキーワードのリスト</param>
        /// <returns>ヒットしたキーワードのインデックス</returns>
        /// <exception cref="System.TimeoutException">指定時間内に条件が達成されなかった</exception>
        /// <exception cref="System.OperationCanceledException">ユーザー操作によるマクロ実行のキャンセル</exception>
        Task<int> WaitAsync(params string[] keywords);

        /// <summary>
        /// 返信が一行来るまで待つ
        /// </summary>
        /// <returns>受信した文字列</returns>
        /// <exception cref="System.TimeoutException">指定時間内に条件が達成されなかった</exception>
        /// <exception cref="System.OperationCanceledException">ユーザー操作によるマクロ実行のキャンセル</exception>
        Task<string> WaitLineAsync();

        /// <summary>
        /// 返信が指定行数来るまで待つ
        /// </summary>
        /// <param name="count">待機する行数</param>
        /// <returns>受信した文字列</returns>
        /// <exception cref="System.TimeoutException">指定時間内に条件が達成されなかった</exception>
        /// <exception cref="System.OperationCanceledException">ユーザー操作によるマクロ実行のキャンセル</exception>
        Task<string[]> WaitLineAsync(int count);
    }
}