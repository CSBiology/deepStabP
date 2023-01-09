module State

open Shared

type Model = {
    Todos: Todo list;
    Input: string
}

type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo
