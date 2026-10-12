import { LeftOutlined } from '@ant-design/icons';
import { Breadcrumb } from 'antd';
import { useNavigate } from 'react-router-dom';

import { _t } from '@/infrastructure/utils/translation';

interface IBreadcrumb {
  link?: string;
  name: string;
}
type Props = {
  items: IBreadcrumb[];
};

export const BreadcrumbUI = (props: Props) => {
  const navigate = useNavigate();

  const { items } = props;
  return (
    <div className={'flex flex-row gap-4 justify-center items-center'}>
      {items.length > 1 ? (
        <LeftOutlined
          onClick={() => {
            items[0].link && navigate(items[0].link);
          }}
        />
      ) : null}
      <Breadcrumb separator='/' className={'font-bold text-20-30 text-[#000000] breadcrumb-css'}>
        {items.map((it, idx) => {
          if (it.link) {
            return (
              <Breadcrumb.Item key={idx} onClick={() => it.link && navigate(it.link)}>
                <p className='cursor-pointer'>{_t(it.name)}</p>
              </Breadcrumb.Item>
            );
          }
          return <Breadcrumb.Item key={idx}>{_t(it.name)}</Breadcrumb.Item>;
        })}
      </Breadcrumb>
    </div>
  );
};
