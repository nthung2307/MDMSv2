import { Layout } from 'antd';
import { ReactNode } from 'react';

const Content = ({ children }: { children: ReactNode }) => {
  return (
    <Layout.Content className='bg-white-FAFBFC overflow-hidden flex-1 flex flex-col'>
      {children}
    </Layout.Content>
  );
};

export default Content;
