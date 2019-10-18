
export class LoginError {
  constructor(public message) { }
}

export class LoginSuccess {
  constructor(public message) { }
}

export class LoginInfo {
  constructor(public message) { }
}

export class TableInfoUpdate {
  constructor(public message) { }
}

export class ClientInfoUpdate {
  constructor(public message) { }
}

export class TableListUpdate {
  constructor(public message) { }
}

export class TableStateUpdate {
  constructor(public tableCode) { }
}

export class JoinGameError {
  constructor(public message) { }
}

export class JoinGameSuccess {
  constructor(public message) { }
}

export class LeaveGameTable {
  constructor(public message) { }
}

export class BaccaratBigSync {
  constructor(public message){ }
}

export class StartBetting {
  constructor(public message){ }
}

export class EndBetting {
  constructor(public message){ }
}

export class SetCard {
  constructor(public message){ }
}

export class VoidCard {
  constructor(public message){ }
}

export class CancelRound {
  constructor(public message){ }
}

export class EndRound {
  constructor(public result){ }
}

export class PlayerMoney {
  constructor(public value){ }
}

export class PlaceBetError {
  constructor(public message){ }
}

export class PlaceBetSuccess {
  constructor(public message){ }
}
