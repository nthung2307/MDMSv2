import { Layout } from 'antd';
import { ReactNode } from 'react';

const Content = ({ children }: { children: ReactNode }) => {
  return (
    <Layout.Content className='flex-1 flex flex-col p-5 min-w-[1200px]'>{children}</Layout.Content>
  );
};

export default Content;
