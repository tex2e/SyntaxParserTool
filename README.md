
# 構文解析・実行ツール

### 依存パッケージ

```
dotnet add package Sprache --version 2.3.1
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Microsoft.Extensions.Logging
dotnet add package System.Text.Encoding.CodePages
```

### 使い方（例）

```
dotnet run --project Parser --input InputFiles/TestFile.cmd --type bat --encode sjis
```

### 単体テスト

```
dotnet test
```
