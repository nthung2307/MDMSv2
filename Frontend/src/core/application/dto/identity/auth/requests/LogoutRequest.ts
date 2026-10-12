export class LogoutRequest {
  access_token: string;
  constructor(arg: { access_token: string }) {
    this.access_token = arg.access_token;
  }
}
