export class LoginRequest {
  userName: string;
  password: string;
  constructor(arg: { userName: string; password: string }) {
    this.userName = arg.userName;
    this.password = arg.password;
  }
}
