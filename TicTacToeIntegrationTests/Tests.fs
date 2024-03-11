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
