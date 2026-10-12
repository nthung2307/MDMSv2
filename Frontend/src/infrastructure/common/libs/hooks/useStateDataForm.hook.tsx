import { useState } from 'react';

export function useStateDataForm(initialState: any) {
  const [_data, _setData] = useState(initialState);
  const data = _data;
  const setData = (newData: Partial<any>) => {
    Object.assign(data, newData);
    _setData({ ...data });
  };
  const resetData = () => {
    _setData(initialState);
  };
  return [data, setData, resetData] as const;
}
