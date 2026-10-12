export class RefreshTokenRequest {
  refresh_token: string;
  constructor(arg: { refresh_token: string }) {
    this.refresh_token = arg.refresh_token;
  }
}
