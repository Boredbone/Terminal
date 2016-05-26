マクロファイルの記述方法
===========



---

## 構成


1)\*.csファイルに，名前空間・クラス・メソッドを記述し，メソッドに`[Macro]`属性を付ける

or

2)\*.csファイルに，メソッドの実装のみを記述する


1)の場合，ファイル内に`[Macro]`属性を持つメソッドが複数記述されていても構いません．
ただし，シグネチャを必ず
```cs
public async Task MethodName(IMacroEngine Macro, IPluginManager Plugins)
```
としてください(メソッド名は任意)．

特に，パラメータ名(Macro, Plugins)もこの通りでなければならないことに注意してください．

また，属性を`[Macro("任意の文字列")]`とすることで，マクロに名前を付けることができます．


---


## 文法

コードはC#6.0の文法によって解釈されます．


---

## アセンブリの参照


スクリプトからは，下記のアセンブリを参照することができます．

```
mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
```

また，プラグインがロードされている場合はそのアセンブリを参照することができます．


---

## usingディレクティブ

スクリプトでは，あらかじめ下記の名前空間のusingディレクティブに相当する設定がなされており，
名前空間の記述を省略することができます．

```cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
```


---

## メソッド外へのアクセス

スクリプト実行の際は，対象となるメソッドの内部のみがコンパイルされます．
そのため，メソッドの外に定義したフィールドや他のメソッドを参照することはできません．

NG

```cs
class MyClass
{

  private string value1 = "abc";

  private string TestMethod1()
  {
      return "def";
  }

  [Macro]
  public async Task MacroMethod(IMacroEngine Macro, IPluginManager Plugins)
  {
      await Macro.SendAsync(value1);

      await Macro.SendAsync(TestMethod1());
  }
}
```

代わりに，ローカル変数およびローカルのデリゲートとラムダ式を使用してください．

OK

```cs
class MyClass
{

  [Macro]
  public async Task MacroMethod(IMacroEngine Macro, IPluginManager Plugins)
  {

    var value1 = "abc";

    Func<string> TestMethod1 = () => 
    {
        return "def";
    };

    await Macro.SendAsync(value1);

    await Macro.SendAsync(TestMethod1());
  }
}
```

---

## 非同期メソッド

マクロのAPIには，TaskまたはTask&lt;T&gt;を返すいくつかの非同期メソッドがあります．
これらの非同期メソッドを使用する際には，awaitキーワードを使用してメソッドの終了を待機してください．

```cs
await Macro.SendAsync("abc");
await Macro.WaitAsync("def");
await Macro.DelayAsync(1000);
```

---

## プラグイン

マクロ内でプラグインを使用することができます．

Plugins.Getメソッドによりプラグインのインスタンスを取得します．

```cs
var plugin = Plugins.Get<PluginNamespace.PluginType>();
```

プラグインの利用方法については，各プラグインの資料を参照してください．

Macro API Reference
======================


C#スクリプトにマクロ機能を提供します．



プロパティ

|名前|説明|
|:---|:---|
|[Timeout](#Timeout0)|タイムアウトを設定|

メソッド

|名前|説明|
|:---|:---|
|[ClearCancellation()](#ClearCancellation0)|キャンセル状態の解除|
|[DelayAsync(int)](#DelayAsync0)|指定時間遅延|
|[Display(string)](#Display0)|画面に文字列を表示|
|[History(int)](#History0)|受信履歴を行ごとに取得|
|[SendAsync(string)](#SendAsync0)|文字列を送信|
|[SendAsync(string, bool)](#SendAsync1)|文字列を送信|
|[SetLogState(bool)](#SetLogState0)|ログの追尾をセット|
|[WaitAsync()](#WaitAsync1)|受信を待機|
|[WaitAsync(params string[])](#WaitAsync0)|受信を待機|
|[WaitLineAsync()](#WaitLineAsync0)|返信が一行来るまで待つ|
|[WaitLineAsync(int)](#WaitLineAsync1)|返信が指定行数来るまで待つ|


------

## プロパティ


<a name ="Timeout0">
### Macro.Timeout Property
```cs
int Timeout { get; set; }
```
通信処理のタイムアウトをミリ秒単位で設定します．

#### 解説

マクロの実行中にタイムアウトが発生すると，`System.TimeoutException`が投げられます．
このプロパティが0に設定されているとき，タイムアウトは発生しません．
初期値は0に設定されています．


例:
```cs
// タイムアウトを無効化
Macro.Timeout = 0;
```
```cs
// 4.0秒のタイムアウトを設定
Macro.Timeout = 4000;

// コマンドを送信
await Macro.SendAsync("Command");

try
{
	// 返信待ち
	await Macro.WaitAsync("Reply");
}
catch (TimeoutException)
{
	// 4.0秒以内に指定した返信が来なかった場合，以下の処理が実行されます
	await Macro.SendAsync("Retry");
}
```

------

## 非同期メソッド


非同期メソッドを使用する際には，`await`演算子を適用します．

以下の非同期メソッドは，ユーザーによってマクロ実行がキャンセルされると
`System.OperationCanceledException`を投げます．
また，メッセージの受信を待機するメソッドは，タイムアウトが設定されている時に
`System.TimeoutException`を投げます．
必要に応じて，`try{ }catch{ }`構文によりこれらの例外をハンドルします．

------

<a name ="SendAsync0">
### Macro.SendAsync Method
```cs
Task SendAsync(string text)
```
文字列を送信します．

#### パラメータ
```cs
string text
```
送信する文字列

#### 戻り値
```cs
Task
```
非同期操作の戻り値はありません．

#### 例外
```cs
System.OperationCanceledException
```

------

<a name ="SendAsync1">
### Macro.SendAsync Method
```cs
Task SendAsync(string text, bool immediately)
```
文字列を送信します．

#### パラメータ
```cs
string text
```
送信する文字列

```cs
bool immediately
```
bool

#### 戻り値
```cs
Task
```
非同期操作の戻り値はありません．

#### 例外
```cs
System.OperationCanceledException
```


------

<a name ="DelayAsync0">
### Macro.DelayAsync Method
```cs
Task DelayAsync(int timeMillisec)
```
マクロの実行を指定時間遅延させます．

#### パラメータ
```cs
int timeMillisec
```
待機する時間(ミリ秒)

#### 戻り値
```cs
Task
```
非同期操作の戻り値はありません．

#### 例外
```cs
System.OperationCanceledException
```

#### 解説
マクロ内ではSystem.Threading.Tasks.Task.Delayメソッドの使用は推奨されません．
代わりにMacro.DelayAsyncメソッドを使用してください．

------

<a name ="WaitAsync1">
### Macro.WaitAsync Method
```cs
Task<string> WaitAsync()
```

何らかの文字列を受信するまで待機します．

#### パラメータ


#### 戻り値
```cs
Task<string>
```
非同期操作が完了すると，ヒットした文字列が返されます．

#### 例外
```cs
System.OperationCanceledException
System.TimeoutException
```

#### 解説

------

<a name ="WaitAsync0">
### Macro.WaitAsync Method
```cs
Task<int> WaitAsync(params string[] keywords)
```

指定した文字列のいずれかを受信するまで待機します．

#### パラメータ
```cs
params string[] keywords
```
待機する文字列を1個以上指定します．

#### 戻り値
```cs
Task<int>
```
非同期操作が完了すると，指定パラメータ配列のうち最初にヒットした文字列のインデックスが返されます．

#### 例外
```cs
System.OperationCanceledException
System.TimeoutException
```

#### 解説

例:
```cs
// 1個のキーワードを待機

// コマンドを送信
await Macro.SendAsync("Command");

// 返信待ち
await Macro.WaitAsync("Reply");
```
```cs
// 2個以上のキーワードを待機

// コマンドを送信
await Macro.SendAsync("Command");

// 返信待ち
var result = await Macro.WaitAsync("Reply0", "Reply1", "Reply2");

// resultの値
// "Reply0"が返信された時 : result = 0
// "Reply1"が返信された時 : result = 1
// "Reply2"が返信された時 : result = 2
```

------

<a name ="WaitLineAsync0">
### Macro.WaitLineAsync Method
```cs
Task<string> WaitLineAsync()
```

改行コードを受信するまで待機します．

#### パラメータ


#### 戻り値
```cs
Task<string>
```
非同期操作が完了すると，ヒットした文字列が返されます．

#### 例外
```cs
System.OperationCanceledException
System.TimeoutException
```

#### 解説

------

<a name ="WaitLineAsync1">
### Macro.WaitLineAsync Method
```cs
Task<string[]> WaitLineAsync(int count)
```

改行コードを指定数受信するまで待機します．

#### パラメータ
```cs
int count
```
行数

#### 戻り値
```cs
Task<string[]>
```
非同期操作が完了すると，ヒットした文字列が返されます．

#### 例外
```cs
System.OperationCanceledException
System.TimeoutException
```

#### 解説

------

## 同期メソッド


<a name ="History0">
### Macro.History Method
```cs
string History(int back)
```
受信履歴を一行ごとに取得します．

#### パラメータ
```cs
int back
```
履歴をさかのぼる行数．
0のとき，最後に受信した行を取得します．

#### 戻り値
```cs
string
```
受信履歴の指定行の文字列


#### 解説

以下のような受信履歴があるとき，
```
aaa
bbb
ccc
>
```
次のコードでそれぞれの行を取得することができます．
```cs
var h0 = Macro.History(0); // h0 = ">"
var h1 = Macro.History(1); // h1 = "ccc"
var h3 = Macro.History(3); // h3 = "aaa"
```



------

<a name ="Display0">
### Macro.Display Method
```cs
void Display(string text)
```
ログ画面に文字列を表示します．

#### パラメータ
```cs
string text
```
表示する文字列

#### 戻り値


#### 解説

-----

<a name ="ClearCancellation0">
### Macro.ClearCancellation Method
```cs
void ClearCancellation()
```
キャンセル状態を解除します．

#### パラメータ


#### 戻り値


#### 解説


-----

<a name ="SetLogState0">
### Macro.SetLogState Method
```cs
void SetLogState(bool state)
```
ログの表示状態を設定します．

#### パラメータ
```cs
bool state
```

#### 戻り値


#### 解説

