# MyLab.DbTest
[![NuGet Version and Downloads count](https://buildstats.info/nuget/MyLab.DbTest)](https://www.nuget.org/packages/MyLab.DbTest)

```
Поддерживаемые платформы: .NET Core 3.1+
```
Ознакомьтесь с последними изменениями в [журнале изменений](/changelog.md).

`MyLab.DbTest` Предоставляет инструменты для использования в тестах на базе `xUnit` с использование БД (`linq2db`).

## Временная база `TmpDbFixture`

Для модульных или функциональных тестов, в которых необходимо использовать БД, приближённую к реальной,  в основном требуется создание изолированной БД для каждого теста. 

`TmpDbFixture` предоставляет возможность создавать временную БД на базе `sqlite`.  

Для инициирования созданной временной БД используются инициализаторы тестовой БД (`ITestDbInitializer`). Пример инициализатора:

```C#
public class TestDbInitializer : ITestDbInitializer
{
    public async Task InitializeAsync(DataConnection dataConnection)
    {
        var t = await dataConnection.CreateTableAsync<Entity>();
        await t.InsertAsync(() => new Entity { Value = "foo" });
        await t.InsertAsync(() => new Entity { Value = "bar" });
    }
}
```

Инициализатор может использоваться на весь тестовый класс (`базовый`) - вызывается каждый раз для каждой созданной БД. Или может быть `дополнительным` и он будет вызывается только один раз для создаваемой БД после вызова базового инициализатора, если он указан. 

Основной инициализатор указывается как `generic` параметр у класса `TmpDbFixture<TInitializer>`:

```c#
public class TmpDbFixtureBehavior : IClassFixture<TmpDbFixture<TestDbInitializer>>
{
}
```

Дополнительный инициализатор указывается явно при создании БД:

```C#
var additionalInitializer = new AdditionalTestDbInitializer();
var mgr = await _fxt.CreateDbAsync(additionalInitializer);
```

Особенности использования инициализаторов:

* их можно не использовать. Например, для тестов на пустой БД
* допускается любая комбинация
* базовый инициализатор выполняется раньше дополнительного
* стоит помнить о коллизиях 

Особенности использования `TmpDbFixture`:

* используется как `test fixture`

* для логирования запросов, необходимо передать в свойство `Output` объект типа `ITestOutputHelper`

* можно указать базовый инициализатор БД 

* можно указать дополнительный инициализатор при создании БД


В примере ниже показано, как использовать этот механизм с базовым инициализатором БД:

```C#
public class TmpDbFixtureBehavior : IClassFixture<TmpDbFixture<TestDbInitializer>>
{
    private readonly TmpDbFixture _fxt;

    public TmpDbFixtureBehavior(ITestOutputHelper outputHelper, TmpDbFixture<TestDbInitializer> fxt)
    {
        fxt.Output = outputHelper;
        _fxt = fxt;
    }

    [Fact]
    public async Task ShouldContainsPredefinedEntities()
    {
        var mgr = await _fxt.CreateDbAsync();

        ...
    }
}
```

Такой подход удобен, если все тесты должны отработать по базе в одном и том же состоянии.

В следующем примере тест использует дополнительный инициализатор без базового:

```C#
public class TmpDbFixtureBehavior : IClassFixture<TmpDbFixture>
{
    private readonly TmpDbFixture _fxt;

    public TmpDbFixtureBehavior(ITestOutputHelper outputHelper, TmpDbFixture fxt)
    {
        fxt.Output = outputHelper;
        _fxt = fxt;
    }

    [Fact]
    public async Task ShouldContainsPredefinedEntities()
    {
        var additionalInitializer = new AdditionalTestDbInitializer();
		var mgr = await _fxt.CreateDbAsync(additionalInitializer);

        ...
    }
}
```

## Удалённая база `RemoteDbFixture`

Для случая, когда для интеграционных тестов необходима отдельная БД, например, развёрнутая специально для теста, используйте наследника класса `RemoteDbFixture`.

Наследник должен определить провайдера БД и строку подключения. Например:

```C#
public class MyTestDbFixture : RemoteDbFixture
{
	public MyTestDbFixture()
        : base(new MySqlPRovider(), "Server=myServerAddress;Database=myDataBase")
        {
        }
}

public class MyTestBehavior : IClassFixture<MyTestDbFixture>
{
    private readonly IDbManager _db;

    public MyTestBehavior(ITestOutputHelper outputHelper, MyTestDbFixture fxt)
    {
        fxt.Output = outputHelper;
        _db = fxt.Manager;
    }

    [Fact]
    public async Task ShouldContainsPredefinedEntities()
    {
        _db.DoOnce().Tab<DbEntity>.Where(...)...
        ...
    }
}
```

Особенности:

* подключается в начале теста и отключается в конце;
* ничего не удаляет перед отключением;
* просто предоставляет доступ к удалённой БД без дополнительного функционала.

## Логирование

Ниже приведён пример лога использования тестовой БД:

```
--  SQLite (asynchronously)
CREATE TABLE [FooTable]
(
	[Id]    INTEGER        NOT NULL PRIMARY KEY AUTOINCREMENT,
	[Value] NVarChar(255)      NULL
)

--  SQLite (asynchronously)
INSERT INTO [FooTable]
(
	[Value]
)
VALUES
(
	'foo'
)

--  SQLite (asynchronously)
INSERT INTO [FooTable]
(
	[Value]
)
VALUES
(
	'bar'
)

--  SQLite (asynchronously)
SELECT
	[t1].[Id],
	[t1].[Value]
FROM
	[FooTable] [t1]
```

