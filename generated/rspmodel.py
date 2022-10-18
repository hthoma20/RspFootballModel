from enum import Enum
from typing import Literal, Optional, Union
from pydantic import BaseModel

class RspChoice(str, Enum):
    ROCK = 'ROCK'
    PAPER = 'PAPER'
    SCISSORS = 'SCISSORS'

class Player(str, Enum):
    home = 'home'
    away = 'away'

class RspResult(BaseModel):
    name: Literal['RSP'] = 'RSP'
    home: RspChoice
    away: RspChoice

Result = Union[RspResult]
