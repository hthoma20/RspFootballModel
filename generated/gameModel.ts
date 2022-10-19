
export type RspChoice = 'ROCK' | 'PAPER' | 'SCISSORS';

export type Player = 'home' | 'away';

export type RspAction = {
    name: 'RSP';
    choice: RspChoice;
};

export type Action = RspAction;

export type RspResult = {
    name: 'RSP';
    home: RspChoice;
    away: RspChoice;
};

export type RollResult = {
    name: 'ROLL';
    player: Player;
    roll: number[];
};

export type Result = RspResult | RollResult;
export type ResultName = Result['name'];
