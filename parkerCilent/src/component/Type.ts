class PocketMessage {
    public content: string | undefined;
    public url: string | undefined;
    public type: number | undefined;//1-文本，2-图片，3-语音，4-视频
    
    public add(msg: string): PocketMessage {
      this.content = msg;
      this.type = 1;
      return this;
    }
    public addImg(url: string): PocketMessage {
      this.url = url;
      this.type = 2
      return this;
    }
    public addVoice(url: string): PocketMessage {
      this.url = url;
      this.type = 3
      return this;
    }
    public addVideo(url: string): PocketMessage {
      this.url = url;
      this.type = 4
      return this;
    }
  }
  export default PocketMessage