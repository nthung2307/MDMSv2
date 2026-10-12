import { Button } from 'antd';
import styled from 'styled-components';

type Props = {
  bg?: string;
  color?: string;
  fontSize?: string;
  border?: string;
  borderRadius?: string;
  minHeight?: string;
  minWidth?: string;
  fontWeight?: string;
};
export const ButtonStyled = styled(Button)<Props>`
  &:hover,
  &:focus,
  & {
    min-width: ${(props) => (props.minWidth ? props.minWidth : '90px')};
    min-height: ${(props) => (props.minHeight ? props.minHeight : '')};
    display: flex;
    justify-content: center;
    align-items: center;
    border-radius: ${(props) => (props.borderRadius ? props.borderRadius : '8px')};
    background: ${(props) => (props.bg ? props.bg : '#D53131')};
    color: ${(props) => (props.color ? props.color : 'white')};
    border: ${(props) => (props.border ? props.border : 'none')};
    font-size: ${(props) => (props.fontSize ? props.fontSize : '16px')};
    font-weight: ${(props) => (props.fontWeight ? props.fontWeight : '600')};
    width: 100%;
    height: 100%;
  }
`;

// export const ButtonCommon = (props: Props) => {
//   const { title, colorClass, icon, onClick, disabled } = props
//   return (
//     //blue-bg: background xanh, chữ trắng
//     //red-bg: background đỏ tía, chữ trắng,
//     //cancel: dùng trong nút cancel, background xám, chữ đen, ko border
//     //close: dùng trong nút close, background trắng, chữ xám, border xám,
//     //blue-border: background xám trắng, chữ xanh, border xanh,
//     //active : dùng trong nút active, background trắng xám, chữ xanh, border xanh,
//     //deactive : dùng trong nút deactive, background trắng xám, chữ đỏ, border đỏ,
//     <div className={`${colorClass}-button-common container-btn-common`}>
//       <Button className="" onClick={onClick} icon={icon} disabled={disabled}>{title}</Button>
//     </div>
//   );
// };
