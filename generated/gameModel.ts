
export type RspChoice = 'ROCK' | 'PAPER' | 'SCISSORS';

export type Player = 'home' | 'away';

export type RspResult = {
    name: 'RSP';
    home: RspChoice;
    away: RspChoice;
};

export type Result = RspResult;
export type ResultName = Result['name'];
