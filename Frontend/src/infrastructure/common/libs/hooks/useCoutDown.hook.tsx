import { useEffect, useState } from 'react';

let timerId: any = null;
export const useCountDown = () => {
  const [_countDown, _setCountDown] = useState<number>(1);
  const requestCountDown = (counter: number) => {
    _setCountDown(counter);
  };

  useEffect(() => {
    timerId = _countDown > 0 && setInterval(() => _setCountDown(_countDown - 1), 1000);

    return () => clearInterval(timerId);
  }, [_countDown]);

  return {
    countDown: _countDown,
    requestCountDown,
  };
};
