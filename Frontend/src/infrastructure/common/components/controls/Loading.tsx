import { LoadingOutlined } from '@ant-design/icons';
import { Spin } from 'antd';
import { SpinSize } from 'antd/lib/spin';
/**
 * Full page loading
 */

type FullLoadingProps = {
  isLoading: boolean;
};
export const FullPageLoading = (props: FullLoadingProps) => {
  const { isLoading } = props;
  return (
    <>
      {isLoading === true ? (
        <div className={'full-page-loading'}>
          <LoadingRegion size={'large'} tip={null} />
        </div>
      ) : null}
    </>
  );
};

/**
 * Loading spin
 */

type LoadingRegionProps = {
  color?: string;
  tip?: React.ReactNode;
  size?: SpinSize;
};
export const LoadingRegion = (props: LoadingRegionProps) => {
  const { tip, size, color } = props;

  return (
    <>
      <Spin tip={tip} size={size} style={{ color: color }} indicator={<LoadingOutlined spin />} />
      {/* <p style={{ color: color }}>Loading</p> */}
    </>
  );
};
