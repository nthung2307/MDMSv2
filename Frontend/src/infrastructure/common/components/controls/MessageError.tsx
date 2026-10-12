// import '@/styles/css/common/input.css';

type Props = {
  isError: boolean;
  message: string;
  cssRelative?: boolean;
};
export const MessageError = (props: Props) => {
  const { isError, message, cssRelative } = props;
  return (
    <div className={`message-error ${cssRelative ? 'relative' : ''}`}>
      {isError && message && message.length ? <div className='message'>{message}</div> : null}
    </div>
  );
};
