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