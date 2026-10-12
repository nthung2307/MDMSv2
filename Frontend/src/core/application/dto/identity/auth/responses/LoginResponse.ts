export class LoginResponse {
  public token: string;
  public refreshToken: string;
  public user: UserData;
  constructor(args: { token: string; refreshToken: string; user: UserData }) {
    this.token = args.token;
    this.refreshToken = args.refreshToken;
    this.user = args.user;
  }
}

class UserData {
  public id: string;
  public email: string;
  public firstName: string;
  public lastName: string;
  public role: string;
  public avatar: string;
  public phone: string;
  public address: string;
  public status: string;
  public createdAt: Date;
  public updatedAt: Date;
  constructor(args: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
    avatar: string;
    phone: string;
    address: string;
    status: string;
    createdAt: Date;
    updatedAt: Date;
  }) {
    this.id = args.id;
    this.email = args.email;
    this.firstName = args.firstName;
    this.lastName = args.lastName;
    this.role = args.role;
    this.avatar = args.avatar;
    this.phone = args.phone;
    this.address = args.address;
    this.status = args.status;
    this.createdAt = args.createdAt;
    this.updatedAt = args.updatedAt;
  }
}
