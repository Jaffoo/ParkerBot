<template>
    <el-container>
        <el-header>
            <el-form>
                <el-form-item>
                    <el-button type="primary" native-type="button" :icon="Setting" @click="config">
                        修改配置
                    </el-button>
                    <el-button type="primary" native-type="button" :icon="Setting" @click="miraiSetting">
                        Mirai配置
                    </el-button>
                    <span style="color:red">(不启用QQ机器人无需配置)</span>
                    <el-button v-if="useMirai" type="primary" native-type="button" :icon="Setting"
                        @click="startMirai">启动Mirai机器人</el-button>
                    <el-button type="primary" native-type="button" :icon="Check" @click="start">启动机器人</el-button>
                    <el-button v-if="useAli" type="primary" native-type="button" :icon="Setting"
                        @click="startAli">启动阿里云盘</el-button>
                </el-form-item>
            </el-form>
        </el-header>
        <el-main>
            <el-row :gutter="20">
                <el-col :span="12">
                    <h3 style="cursor: pointer;margin-left: 40%;">即时消息及日志</h3>
                    <div style="height: 500px;overflow:auto;" id="textArae">
                        <div v-for="(item, index) in log" style="margin:5px">{{ (index + 1) + ':' + item }}</div>
                    </div>
                </el-col>
                <el-col :span="12">
                    <h3 title="点此刷新" @click="refresh" style="cursor: pointer;margin-left: 40%;">图片待审核列表</h3>
                    <div style="height: 500px;overflow:auto;">
                        <table>
                            <tr>
                                <th width="300px">图片</th>
                                <th>操作</th>
                            </tr>
                            <tr v-for="item, index in pic">
                                <td align="center">
                                    <el-image style="height: 80px;width: 80px;" :src="item.content"
                                        :preview-src-list="[item.content]"></el-image>
                                </td>
                                <td>
                                    <el-button size="small" @click="checkTrue(item.id, index)">保存</el-button>
                                    <el-button size="small" type="danger" @click="checkFalse(item.id, index)">删除</el-button>
                                </td>
                            </tr>
                        </table>
                    </div>
                </el-col>
            </el-row>
        </el-main>
    </el-container>
</template>
<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import { useRouter } from "vue-router";
import { Setting, Check } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import axios from "axios";
import QChatSDK from "nim-web-sdk-ng/dist/QCHAT_BROWSER_SDK";
import NIMSDK from "nim-web-sdk-ng/dist/NIM_BROWSER_SDK";
import type { SubscribeAllChannelResult } from "nim-web-sdk-ng/dist/QCHAT_BROWSER_SDK/QChatServerServiceInterface";
import dayjs from 'dayjs';
import NimChatroomSocket from '../component/Live'

const baseConfig = ref({} as any);
const mirai = ref({} as any);
const useMirai = ref(false);
const useAli = ref(false);
const router = useRouter();
const nim = ref<NIMSDK>();
const qChat = ref<QChatSDK>();
const log = ref<Array<string>>(new Array<string>());
const pic = ref<Array<string>>(new Array<string>());
const ws = ref<WebSocket>();
const wsReady = ref<boolean>(false);

watch(log.value, (newVal, OldVal) => {
    if (newVal.length >= 100) {
        log.value.splice(0, 1);
    }
    var logDiv: HTMLElement | null = document.getElementById("textArae");
    if (logDiv === null) return;
    logDiv.scrollTop = logDiv.scrollHeight;
})

const startAli = async () => {
    await axios({
        url: "http://parkerbot.api/api/StartAliYunApi"
    });
}

const startMirai = async () => {
    var res = await axios({
        url: "http://parkerbot.api/api/StartMiraiConsole"
    });
    if (!res.data) {
        ElMessage({
            showClose: true,
            message: 'Mirai目录不存在或路径错误，请检查后重试！',
            type: 'error'
        });
    }
    if (res.data) {
        ElMessage({
            showClose: true,
            message: '已为您打开Mirai控制台！',
            type: 'success'
        });
    }
};
const start = async () => {
    // const liveNim = new NimChatroomSocket({ roomId: '2615022451', onMessage: liveMsg })
    // console.log(liveNim)
    // liveNim.init('NjMyZmVmZjFmNGM4Mzg1NDFhYjc1MTk1ZDFjZWIzZmE=');
    // setTimeout(() => {
    // liveNim.disconnect();
    // }, 5000);
    // return;
    if (nim.value) {
        nim.value.destroy()
    }
    if (qChat.value) {
        qChat.value.destroy()
    }
    const res = await getConfig();
    baseConfig.value = res.data.config;
    const myConfig = res.data.config.KD;

    var useKd: boolean = false;
    res.data.enable.forEach((item: any) => {
        if (item.key == "KD" && item.value == "True") {
            useKd = true;
        }
    });
    if (useKd) {
        nim.value = new NIMSDK({
            appkey: atob(myConfig.appKey),
            account: myConfig.account,
            token: myConfig.token,
        });
        await nim.value.connect();
        qChat.value = new QChatSDK({
            appkey: atob(myConfig.appKey),
            account: myConfig.account,
            token: myConfig.token,
            linkAddresses: await nim.value.plugin.getQChatAddress({
                ipType: 2,
            }),
        })

        qChat.value.on("logined", handleLogined);
        qChat.value.on("message", handleMessage);
        qChat.value.on("disconnect", handleRoomSocketDisconnect);
        await qChat.value.login();
    } else {
        var msg = "机器人启动中！";
        log.value.push(msg);
        msg = "";
        var res1 = await axios({ url: "http://parkerbot.api/api/start" });
        if (useMirai.value) {
            if (res1.data.mirai) {
                msg += "QQ机器人启动成功。";
            } else {
                msg += "QQ机器人启动失败。";
            }
        }
        setTimeout(() => {
            if (msg) log.value.push(msg);
            log.value.push("机器人已启动");
        }, 500);
    }
};

const handleLogined = async function () {
    var msg = "机器人启动中！口袋登录成功。开始订阅小偶像聊天服务器，";
    if (qChat.value == null) throw ("聊天室未成功实例化");
    const result: SubscribeAllChannelResult =
        await qChat.value.qchatServer.subscribeAllChannel({
            type: 1,
            serverIds: [baseConfig.value.KD.serverId],
        });
    if (result.failServerIds.length) {
        msg += `订阅服务器【${result.failServerIds[0]}】失败。`;
        return;
    }
    msg += `订阅服务器【${baseConfig.value.KD.serverId}】成功。`;
    var res = await axios({ url: "http://parkerbot.api/api/start" });
    ws.value = new window.WebSocket("ws://localhost:6001");
    ws.value.onopen = () => {
        wsReady.value = true;
        log.value.push("连接WebSocket服务器成功。");
    };
    ws.value.onclose = () => {
        wsReady.value = false;
        log.value.push("连接WebSocket服务器失败。");
    };
    if (useMirai.value) {
        if (res.data.mirai) {
            msg += "QQ机器人启动成功。";
        } else {
            msg += "QQ机器人启动失败。";
        }
    }
    log.value.push(msg);
};

const handleMessage = async function (msg: any) {
    msg.ext = JSON.parse(msg.ext as string);
    msg.channelName = await getChannel(msg.channelId);
    msg.time = dayjs(msg.time).format("YYYY-MM-DD HH:mm:ss");
    if (wsReady.value) {
        ws.value?.send(JSON.stringify(msg));
    }
    //#region 直播
    if (msg.channelName == "直播") {
        console.log(msg);
        // const liveNim = new NimChatroomSocket({ roomId: baseConfig.value.KD.roomId, onMessage: liveMsg })
        // liveNim.init(baseConfig.value.KD.appKey);
    }
    //#endregion
    var mess = `【${msg.channelName}|${msg.time}】${msg.ext.user.nickName}:${msg.body}`;
    log.value.push(mess);
};

const liveMsg = function (t: any, event: any) {
    console.log("liveMsgT", t);
    console.log("liveMsgEvent", event);
}

const handleRoomSocketDisconnect = function (...context: any): void {
    log.value.push("登录连接状态已断开。");
};

const getChannel = async function (id: number) {
    if (qChat.value == null) throw ("聊天室未成功实例化。");
    const channelResult = await qChat.value.qchatChannel.getChannels({
        channelIds: [`${id}`],
    });
    if (channelResult) {
        return channelResult[0].name;
    }
    return "";
};

const config = () => {
    if (nim.value) {
        nim.value.destroy()
    }
    if (qChat.value) {
        qChat.value.destroy()
    }
    router.push("/config");
};
const miraiSetting = () => {
    if (nim.value) {
        nim.value.destroy()
    }
    if (qChat.value) {
        qChat.value.destroy()
    }
    router.push("mirai");
};

const getConfig = async (): Promise<any> => {
    var res = await axios({
        url: "http://parkerbot.api/api/GetBaseConfig"
    });
    return res;
};
onMounted(async () => {
    const res = await getConfig();
    baseConfig.value = res.data.config;
    mirai.value = res.data.mirai;
    useMirai.value = res.data.mirai.useMirai;
    res.data.enable.forEach((item: any) => {
        if (item.key === "BD") {
            if (JSON.parse(item.value) && res.data.config.BD.saveAliyunDisk) {
                useAli.value = true;
            }
        }
    })
    await refresh();
});

const checkTrue = async (id: number, index: number) => {
    //传给后端
    var res = await axios({
        url: "http://parkerbot.api/api/piccheck?id=" + id + "&type=1"
    });
    if (res.data) {
        ElMessage({
            showClose: true,
            message: '保存成功！',
            type: 'success'
        });
        pic.value.splice(index, 1);
    }
    if (!res.data) {
        ElMessage({
            showClose: true,
            message: '保存失败！',
            type: 'error'
        });
    }
};

const checkFalse = async (id: number, index: number) => {
    //传给后端
    var res = await axios({
        url: "http://parkerbot.api/api/piccheck?id=" + id + "&type=2"
    });
    if (res.data) {
        ElMessage({
            showClose: true,
            message: '保存成功！',
            type: 'success'
        });
        pic.value.splice(index, 1);
    }
    if (!res.data) {
        ElMessage({
            showClose: true,
            message: '保存失败！',
            type: 'error'
        });
    }
};

const refresh = async () => {
    var res = await axios({
        url: "http://parkerbot.api/api/refresh"
    });
    pic.value = res.data;
}
</script>