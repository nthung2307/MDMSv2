import { Layout } from 'antd';

import Content from '@/infrastructure/common/components/layout/Content';
import Header from '@/infrastructure/common/components/layout/Header';
import LeftMenu from '@/infrastructure/common/components/layout/LeftMenu';

const MainLayout = ({ children }: { children: React.ReactNode }) => {
  return (
    <Layout className='fixed top-0 bottom-0 left-0 right-0'>
      <Header />
      <Layout>
        <LeftMenu />
        <Layout>
          <Content children={children} />
        </Layout>
      </Layout>
    </Layout>
  );
};

export default MainLayout;
