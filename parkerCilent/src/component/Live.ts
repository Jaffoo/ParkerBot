import NIM_SDK from '@yxim/nim-web-sdk/dist/SDK/NIM_Web_SDK.js';
import type NIM_Web_Chatroom from '@yxim/nim-web-sdk/dist/SDK/NIM_Web_Chatroom';
import type { LiveRoomMessage } from './messageType';

type OnMessage = (t: NimChatroomSocket, event: Array<LiveRoomMessage>) => void | Promise<void>;

interface NimChatroomSocketArgs {
  liveId: string;
  onMessage: OnMessage;
}

interface NIMError {
  code: number | string;
  message: string;
}

/* 创建网易云信sdk的socket连接 */
class NimChatroomSocket {
  public liveId: string;
  public nimChatroomSocket: NIM_Web_Chatroom | undefined;
  public onMessage: OnMessage;
  constructor(arg: NimChatroomSocketArgs) {
    this.liveId = arg.liveId; // 房间id
    this.onMessage = arg.onMessage;
  }

  // 初始化
  init(appkey: string): void {
    this.nimChatroomSocket = NIM_SDK.Chatroom.getInstance({
      appKey: atob(appkey),
      chatroomId: this.liveId,
      chatroomAddresses: ['chatweblink01.netease.im:443'],
      onconnect: this.onConnet,
      onmsgs: this.handleRoomSocketMessage,
      onerror: this.handleRoomSocketError,
      ondisconnect: this.handleRoomSocketDisconnect,
      isAnonymous: true,
      chatroomNick: this.uuid(),
      chatroomAvatar: '',
      db: false,
      dbLog: false
    });
  }

  // 事件监听
  handleRoomSocketMessage: Function = (event: Array<LiveRoomMessage>): void => {
    this.onMessage(this, event);
  };

  // 事件监听
  onConnet: Function = (event: any): void => {
    let msg = `进入小偶像的直播间成功。`;
    console.log(msg);
  };

  // 进入房间失败
  handleRoomSocketError: Function = (err: NIMError, event: any): void => {
    console.log('发生错误', err, event);
  };

  // 断开连接
  handleRoomSocketDisconnect: Function = (err: NIMError): void => {
    console.log('连接断开', err);
  };

  // 断开连接
  disconnect(): void {
    this.nimChatroomSocket?.disconnect?.({ done(): void { /* noop */ } });
    this.nimChatroomSocket = undefined;
  }

  uuid(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
      var r = Math.random() * 16 | 0
      var v = c === 'x' ? r : (r & 0x3 | 0x8)
      return v.toString(16)
    })
  }
}

export default NimChatroomSocket;