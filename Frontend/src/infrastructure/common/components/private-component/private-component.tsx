import { getAccountRole } from '@/infrastructure/utils/helpers.ts';

interface PrivateComponentProps {
  children: React.ReactNode;
  roles: string[];
}

const PrivateComponent = ({ children, roles }: PrivateComponentProps) => {
  const role = getAccountRole();
  if (!roles.includes(role)) {
    return null;
  }
  return <>{children}</>;
};

export default PrivateComponent;
