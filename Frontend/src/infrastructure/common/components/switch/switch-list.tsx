import { Switch } from 'antd';
import { useEffect, useState } from 'react';

type SwitchProps = {
  disabled: boolean;
  dataAttribute?: boolean;
  onSwitch(check: boolean): void;
  attribute: string;
  setData(data: any): void;
};

const SwitchList = (props: SwitchProps) => {
  const { disabled = false, onSwitch, dataAttribute = false, attribute, setData } = props;

  const [toggle, setToggle] = useState<boolean>(false);

  const onChange = (checked: boolean) => {
    setToggle(checked);
    onSwitch(checked);
    setData({
      [attribute]: checked,
    });
  };

  useEffect(() => {
    setToggle(dataAttribute);
  }, [dataAttribute]);

  return <Switch onChange={onChange} value={toggle} disabled={disabled}></Switch>;
};
export default SwitchList;
