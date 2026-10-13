import styled from 'styled-components';

interface IStatusTagCommon {
  bgColor: string;
  textColor: string;
}

export const StatusTagCommonStyled = styled.div<IStatusTagCommon>`
  // background-color: ${({ bgColor }) => bgColor};
  color: ${({ bgColor }) => bgColor};
  font-weight: 700;
  width: fit-content;
  display: flex;
  flex-direction: row;
  justify-content: center;
  border-radius: 4px;
  padding: 8px 16px;
`;

type Props = {
  status: string;
  bgColor?: string;
  textColor?: string;
};

const StatusTagCommon = (props: Props) => {
  const { status, bgColor = '#41A026', textColor = 'white' } = props;
  return (
    <StatusTagCommonStyled
      className={`flex flex-row justify-center py-1 rounded-2xl`}
      bgColor={bgColor}
      textColor={textColor}
    >
      <p className={'text-green-0F860F'}>{status}</p>
    </StatusTagCommonStyled>
  );
};

export default StatusTagCommon;
