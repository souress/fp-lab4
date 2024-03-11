# Функциональное программирование. Лабораторная работа №4.

Выполнил: Клименко Кирилл Владимирович

Вариант: Крестики-Нолики.


Цель: получить навыки работы со специфичными для выбранной технологии/языка программирования приёмами.


## Реализация
Для описания интерфейса игры было решено взять фреймфорк Avalonia, а именно Avalonia.FuncUI - надстройка над оригинальным фреймворком для написание кода в функциональном стиле.
```fsharp
module TicTacToe

open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Layout

type private PieceStyle =
    | X
    | O
    | Blank

type private Status =
    | Run
    | Stop

type private GameState =
    { BoardState: IWritable<PieceStyle[,]>
      TextBlockState: IWritable<string>
      Player: IWritable<PieceStyle>
      Status: IWritable<Status> }

type private Messages =
    { Initial: string
      Won: PieceStyle -> string
      SwitchError: string }

let private createEmptyGrid (rows: int) (cols: int) =
    Array2D.create rows cols PieceStyle.Blank

let private messages =
    { Initial = "Let's play"
      Won = (fun piece -> $"Player '{piece}' won!")
      SwitchError = $"Player must not be '{PieceStyle.Blank}'" }

let private switchPlayer (player: PieceStyle) =
    match player with
    | PieceStyle.X -> PieceStyle.O
    | PieceStyle.O -> PieceStyle.X
    | Blank -> failwith messages.SwitchError

let private checkWin (player: PieceStyle) (gameState: GameState) =
    let checkRow (row: int) =
        List.forall (fun col -> gameState.BoardState.Current[row, col] = player) [ 0..2 ]

    let checkColumn (col: int) =
        List.forall (fun row -> gameState.BoardState.Current[row, col] = player) [ 0..2 ]

    let checkDiagonal () =
        List.forall (fun i -> gameState.BoardState.Current[i, i] = player) [ 0..2 ]

    let checkAntiDiagonal () =
        List.forall (fun i -> gameState.BoardState.Current[i, 2 - i] = player) [ 0..2 ]

    List.exists checkRow [ 0..2 ]
    || List.exists checkColumn [ 0..2 ]
    || checkDiagonal ()
    || checkAntiDiagonal ()

let private handleClick (row: int) (col: int) (gameState: GameState) =
    match gameState.Status.Current with
    | Run ->
        let currentPlayer = gameState.Player.Current

        if gameState.BoardState.Current[row, col] = PieceStyle.Blank then
            gameState.BoardState.Current[row, col] <- currentPlayer

            if checkWin currentPlayer gameState then
                currentPlayer |> messages.Won |> gameState.TextBlockState.Set
                Status.Stop |> gameState.Status.Set

        gameState.Player.Current |> switchPlayer |> gameState.Player.Set
        gameState.BoardState.Current |> gameState.BoardState.Set
    | Stop -> ()

let private handleReset (gameState: GameState) =
    createEmptyGrid 3 3 |> gameState.BoardState.Set
    messages.Initial |> gameState.TextBlockState.Set
    PieceStyle.X |> gameState.Player.Set
    Status.Run |> gameState.Status.Set

let private createButton (row: int) (col: int) (gameState: GameState) =
    Button.create
        [ Button.width 64
          Button.height 64
          Button.horizontalContentAlignment HorizontalAlignment.Center
          Button.verticalContentAlignment VerticalAlignment.Center
          Button.content gameState.BoardState.Current[row, col]
          Button.onClick (fun _ -> handleClick row col gameState) ]

let view =
    Component(fun ctx ->
        let emptyGrid = createEmptyGrid 3 3

        let gameState =
            { BoardState = ctx.useState emptyGrid
              TextBlockState = ctx.useState messages.Initial
              Player = ctx.useState PieceStyle.X
              Status = ctx.useState Status.Run }

        DockPanel.create
            [ DockPanel.verticalAlignment VerticalAlignment.Center
              DockPanel.horizontalAlignment HorizontalAlignment.Center

              DockPanel.children
                  [ TextBlock.create
                        [ TextBlock.dock Dock.Top
                          TextBlock.fontSize 20.0
                          TextBlock.horizontalAlignment HorizontalAlignment.Center
                          TextBlock.text gameState.TextBlockState.Current
                          TextBlock.margin 10 ]
                    Button.create
                        [ Button.dock Dock.Bottom
                          Button.margin 10
                          Button.width 100
                          Button.height 50
                          Button.horizontalAlignment HorizontalAlignment.Center
                          Button.horizontalContentAlignment HorizontalAlignment.Center
                          Button.verticalContentAlignment VerticalAlignment.Center
                          Button.content "Reset"
                          Button.onClick (fun _ -> handleReset gameState) ]
                    StackPanel.create
                        [ StackPanel.dock Dock.Bottom
                          StackPanel.orientation Orientation.Vertical
                          StackPanel.horizontalAlignment HorizontalAlignment.Center
                          StackPanel.verticalAlignment VerticalAlignment.Center
                          StackPanel.children
                              [ for row in 0..2 ->
                                    StackPanel.create
                                        [ StackPanel.orientation Orientation.Horizontal
                                          StackPanel.children [ for col in 0..2 -> createButton row col gameState ] ] ] ] ] ])
```
## Внешний вид
![image](https://github.com/souress/fp-lab4/assets/71097848/b16ce94c-4773-44c7-bc32-31b8652ca718)
![image](https://github.com/souress/fp-lab4/assets/71097848/e7cefc5f-44b3-4739-a034-196b932a5718)

## Тестирование UI
В соответствии с заданием необходимо было реализовать UI тесты для мини-игры. Не удалось найти фреймворки или библиотеки, позволяющие
протестировать UI десктопного приложения, при том что тесты должны были быть написаны в функциональном стиле на языке F#
(есть примеры на C#, но они не подошли по требованиям https://github.com/AvaloniaUI/Avalonia/blob/master/tests/Avalonia.IntegrationTests.Appium/ButtonTests.cs).
Также не подошло по условию и веб-игра, так как остутствуют фреймворки и библиотеки для описания интерфейса на языке F#.
Поэтому было решено написать десктопную мини-игру и отдельно интеграционные тесты на пользовательский интерфейс игры с сайта
https://sethclydesdale.github.io/tic-tac-toe/. Тесты были написаны с использованием фреймворка Canopy, что является оберткой над Selenium.

Примеры тестов:
```fsharp
module Tests

open Xunit
open canopy.parallell

[<Fact>]
let ``State should be initial on first run`` () =
    use browser = functions.start canopy.types.ChromeHeadless
    functions.url "https://sethclydesdale.github.io/tic-tac-toe/" browser

    functions.click "#start-game" browser
    functions.click "/html/body/div[2]/button[3]" browser

    Assert.Equal("X: 0", (functions.element "#score-x" browser).Text)
    Assert.Equal("Draw: 0", (functions.element "#score-draw" browser).Text)
    Assert.Equal("O: 0", (functions.element "#score-o" browser).Text)

[<Fact>]
let ``First move should be X`` () =
    use browser = functions.start canopy.types.ChromeHeadless
    functions.url "https://sethclydesdale.github.io/tic-tac-toe/" browser

    functions.click "#start-game" browser
    functions.click "/html/body/div[2]/button[3]" browser

    Assert.Equal("X's turn", (functions.element "#game-turn" browser).Text)

[<Fact>]
let ``Click on cell should change it's value`` () =
    use browser = functions.start canopy.types.ChromeHeadless
    functions.url "https://sethclydesdale.github.io/tic-tac-toe/" browser

    functions.click "#start-game" browser
    functions.click "/html/body/div[2]/button[3]" browser

    functions.click "#s4" browser

    Assert.Equal("X", (functions.element "/html/body/div[5]/div[3]/div[2]/div[2]/span" browser).Text)

[<Fact>]
let ``First move should switch players`` () =
    use browser = functions.start canopy.types.ChromeHeadless
    functions.url "https://sethclydesdale.github.io/tic-tac-toe/" browser

    functions.click "#start-game" browser
    functions.click "/html/body/div[2]/button[3]" browser

    Assert.Equal("X's turn", (functions.element "#game-turn" browser).Text)
    functions.click "#s4" browser

    Assert.Equal("O's turn", (functions.element "#game-turn" browser).Text)

[<Fact>]
let ``Double click on exact same cell should not switch players`` () =
    use browser = functions.start canopy.types.ChromeHeadless
    functions.url "https://sethclydesdale.github.io/tic-tac-toe/" browser

    functions.click "#start-game" browser
    functions.click "/html/body/div[2]/button[3]" browser

    Assert.Equal("X's turn", (functions.element "#game-turn" browser).Text)
    functions.click "#s4" browser
    Assert.Equal("O's turn", (functions.element "#game-turn" browser).Text)
    functions.click "#s4" browser
    Assert.Equal("O's turn", (functions.element "#game-turn" browser).Text)

[<Fact>]
let ``Counter X wins should increment on X win`` () =
    use browser = functions.start canopy.types.ChromeHeadless
    functions.url "https://sethclydesdale.github.io/tic-tac-toe/" browser

    functions.click "#start-game" browser
    functions.click "/html/body/div[2]/button[3]" browser

    functions.click "#s0" browser // X
    functions.click "#s4" browser // O
    functions.click "#s3" browser // X
    functions.click "#s7" browser // O
    functions.click "#s6" browser // Win X

    Assert.Equal("X WINS!", (functions.element "#game-turn" browser).Text)
    Assert.Equal("X: 1", (functions.element "#score-x" browser).Text)

[<Fact>]
let ``Counter Draw should increment on draw`` () =
    use browser = functions.start canopy.types.ChromeHeadless
    functions.url "https://sethclydesdale.github.io/tic-tac-toe/" browser

    functions.click "#start-game" browser
    functions.click "/html/body/div[2]/button[3]" browser

    functions.click "#s8" browser // X
    functions.click "#s7" browser // O
    functions.click "#s6" browser // X
    functions.click "#s2" browser // O
    functions.click "#s5" browser // X
    functions.click "#s3" browser // O
    functions.click "#s4" browser // X
    functions.click "#s0" browser // O
    functions.click "#s1" browser // Draw (X)

    Assert.Equal("DRAW!", (functions.element "#game-turn" browser).Text)
    Assert.Equal("Draw: 1", (functions.element "#score-draw" browser).Text)

[<Fact>]
let ``Counter O wins should increment on O win`` () =
    use browser = functions.start canopy.types.ChromeHeadless
    functions.url "https://sethclydesdale.github.io/tic-tac-toe/" browser

    functions.click "#start-game" browser
    functions.click "/html/body/div[2]/button[3]" browser

    functions.click "#s4" browser // X
    functions.click "#s0" browser // O
    functions.click "#s1" browser // X
    functions.click "#s3" browser // O
    functions.click "#s2" browser // X
    functions.click "#s6" browser // Win O

    Assert.Equal("O WINS!", (functions.element "#game-turn" browser).Text)
    Assert.Equal("O: 1", (functions.element "#score-o" browser).Text)
```


## Вывод

В результате выполнения данной лабораторной работы мною был получен практический опыт применения приёмов функционального
программирования, было ощущение что описываю интерфейс простым текстом. В целом процесс и стиль для меня показался похож на React.
Сложности возникли в момент выбора фреймворка и решение будет ли приложение в вебе или десктопным пришло окончательно только в конце,
после уймы провалившихся попыток найти библиотеку/фреймворк для написания кода на чистом F#, без использования того же C#.
