namespace Votes

type Animal =
    | Cat
    | Dog

type Vote =
    { Animal: Animal
      Email: Option<string> }

type Votes =
    { Cats: int
      Dogs: int }

    member this.Vote(animal: Animal) =
        match animal with
        | Animal.Dog -> { this with Dogs = this.Dogs + 1 }
        | Animal.Cat -> { this with Cats = this.Cats + 1 }

    static member empty: Votes =
        { Cats = 0
          Dogs = 0 }