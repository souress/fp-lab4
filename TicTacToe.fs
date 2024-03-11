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