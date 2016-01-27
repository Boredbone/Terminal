シリアル通信アプリ

下のテキストボックスに文字列を入力，SendボタンクリックまたはEnterキーで送信


## マクロ機能

C#で記述したスクリプトを使用して自動操作可能
詳細はApi\README.md, Api\script.md, Terminal\Models\Macro\MacroSample.cs参照


## プラグイン機能

実行ファイル(Terminal.exe)と同じディレクトリにpluginsフォルダを作り，dllを入れておくことで
プラグインとして実行可能

プラグインとして使用するdll内にはTerminal.Macro.Api.IActivatorを実装したクラスが一つある必要がある
そのクラスには[Export(typeof(IActivator))]属性を付けること

例

```cs
[Export(typeof(IActivator))]
public class Activator : IActivator
{
    private string name = "Plugin1";
    public string Name => this.name;

    public Func<OpenWindowRequestArgs, object> OpenWindowRequested { get; set; }


    public Activator()
    {
    }


    public IPlugin Activate(IMacroPlayer player)
    {
        return new Analyzer(player);
    }

    public void Dispose()
    {
    }

    public bool LaunchUI(LaunchUiArgs args)
    {
        return true;
    }
}
```


その他，プラグインから参照するアセンブリもpluginsフォルダ内に置いておく

IActivator.Activateメソッドの戻り値(IPlugin)はマクロ内から利用するためのオブジェクト

IActivator.LaunchUIメソッドによりプラグインのUIを表示する(戻り値未使用)

IActivator.OpenWindowRequestedにはプラグインがUIを表示するためのデリゲートが格納される
