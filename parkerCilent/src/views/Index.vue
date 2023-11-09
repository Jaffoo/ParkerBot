<template>
    <el-container>
        <el-header height="50px">
            <el-button type="primary" native-type="button" :icon="Setting" @click="config">
                修改配置
            </el-button>
            <el-button type="primary" native-type="button" :icon="Setting" @click="miraiSetting" v-if="!windStatus">
                Mirai配置
            </el-button>
            <span style="color:red">(不启用QQ机器人无需配置)</span>
            <el-button v-if="useMirai" type="primary" native-type="button" @click="startMirai">启动Mirai机器人</el-button>
            <el-button type="primary" native-type="button" @click="start">启动机器人</el-button>
            <el-button v-if="useAli" type="primary" native-type="button" @click="startAli">启动阿里云盘</el-button>
        </el-header>
        <el-dropdown style="right: 1vw;position: fixed;">
            <el-button type="primary">更多
                <el-icon>
                    <ArrowDownBold />
                </el-icon>
            </el-button>
            <template #dropdown>
                <el-dropdown-menu>
                    <el-dropdown-item @click="saveBlogByid">抓取微博</el-dropdown-item>
                    <el-dropdown-item @click="windControl">
                        <span v-if="windStatus === false" style="color: green;">开启风控</span>
                        <span v-else style="color: orange;">关闭风控</span>
                    </el-dropdown-item>
                </el-dropdown-menu>
            </template>
        </el-dropdown>
        <el-main>
            <el-row :gutter="20">
                <el-col :span="12">
                    <h3 style="margin-left: 40%;">消息及日志</h3>
                    <div style="height: 530px;overflow:auto;" id="textArae">
                        <div v-for="(item, index) in log" style="margin:5px">
                            {{ (index + 1) + ':' }}
                            <span v-if="item.type != 2">{{ item.content }}</span>
                            <span v-if="item.type == 2">
                                {{ item.content }}
                                <el-image style="height: 80px;width: 80px;" :src="item.url"
                                    :preview-src-list="[item.url]"></el-image>
                            </span>
                        </div>
                    </div>
                </el-col>
                <el-col :span="12">
                    <h3 title="点此刷新" @click="refresh" style="cursor: pointer;margin-left: 40%;">图片列表</h3>
                    <div style="height: 530px;overflow:auto;">
                        <table>
                            <tr>
                                <th width="300px">图片</th>
                                <th>操作</th>
                            </tr>
                            <tr v-for="(item, index) in pic">
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
    <span title="点击复制"
        style="position: absolute;right: 10px;bottom: 0;color:rgb(194, 191, 191);font-size: 15px;">如发现问题，反馈QQ：1615842006</span>
</template>
<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import { useRouter } from "vue-router";
import { ArrowDownBold, Setting } from '@element-plus/icons-vue';
import axios from "axios";
import QChatSDK from "nim-web-sdk-ng/dist/QCHAT_BROWSER_SDK";
import NIMSDK from "nim-web-sdk-ng/dist/NIM_BROWSER_SDK";
import type { SubscribeAllChannelResult } from "nim-web-sdk-ng/dist/QCHAT_BROWSER_SDK/QChatServerServiceInterface";
import dayjs from 'dayjs';
import NimChatroomSocket from '../component/Live'
import type { LiveRoomMessage } from "@/component/messageType";
import PocketMessage from "@/component/Type";
import { ElMessage } from 'element-plus'

const windStatus = ref<boolean>(false)
const baseConfig = ref({} as any);
const mirai = ref({} as any);
const useMirai = ref(false);
const useAli = ref(false);
const router = useRouter();
const nim = ref<NIMSDK>();
const qChat = ref<QChatSDK>();
const liveNim = ref<NimChatroomSocket>();
const log = ref<Array<PocketMessage>>(new Array<PocketMessage>());
const pic = ref<Array<any>>(new Array<any>());
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
        if (item.key == "KD") {
            useKd = JSON.parse(item.value);
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
        log.value.push(new PocketMessage().add(msg));
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
            if (msg) log.value.push(new PocketMessage().add(msg));
            log.value.push(new PocketMessage().add("机器人已启动"));
        }, 500);
    }
};

const handleLogined = async function () {
    var msg = `口袋登录成功。订阅小偶像${baseConfig.value.KD.name}的房间。`;
    log.value.push(new PocketMessage().add(msg));
    if (qChat.value == null) throw ("聊天室未成功实例化");
    const result: SubscribeAllChannelResult =
        await qChat.value.qchatServer.subscribeAllChannel({
            type: 1,
            serverIds: [baseConfig.value.KD.serverId],
        });
    if (result.failServerIds.length) {
        msg = `小偶像${baseConfig.value.KD.name}的房间订阅失败。请检查配置后重试，如仍有问题，请联系开发者。`;
        log.value.push(new PocketMessage().add(msg));
        return;
    }
    msg = `小偶像${baseConfig.value.KD.name}的房间订阅成功。`;
    log.value.push(new PocketMessage().add(msg));
    //同时订阅直播间
    liveNim.value = new NimChatroomSocket({ liveId: baseConfig.value.KD.liveRoomId, onMessage: liveMsg })
    liveNim.value.init(baseConfig.value.KD.appKey);

    var res = await axios({ url: "http://parkerbot.api/api/start" });
    ws.value = new window.WebSocket("ws://localhost:6001");
    ws.value.onopen = () => {
        wsReady.value = true;
        log.value.push(new PocketMessage().add("连接消息推送服务器成功。"));
    };
    ws.value.onclose = () => {
        wsReady.value = false;
        log.value.push(new PocketMessage().add("连接消息推送服务器失败，请尝试再次启动机器人。请检查配置后重试，如仍有问题，请联系开发者。"));
    };
    if (useMirai.value) {
        if (res.data.mirai) {
            msg = "QQ机器人启动成功。";
        } else {
            msg = "QQ机器人启动失败，请尝试再次启动机器人。请检查配置后重试，如仍有问题，请联系开发者。";
        }
        log.value.push(new PocketMessage().add(msg));
    }
};

const handleMessage = async function (msg: any) {
    msg.fromType = 1;
    msg.ext = JSON.parse(msg.ext as string);
    msg.channelName = await getChannel(msg.channelId);
    msg.time = dayjs(msg.time).format("YYYY-MM-DD HH:mm:ss");
    if (wsReady.value) {
        ws.value?.send(JSON.stringify(msg));
    }
    if (msg.type == "text") {
        var mess = `【${msg.channelName}|${msg.time}】${msg.ext.user.nickName}:${msg.body}`;
        log.value.push(new PocketMessage().add(mess));
    }
    else if (msg.type == "image") {
        log.value.push(new PocketMessage().addImg(`【${msg.channelName}|${msg.time}】${msg.ext.user.nickName}:`, msg?.attach?.url));
    }
    else if (msg.type == "video") {
        log.value.push(new PocketMessage().addVideo(`【${msg.channelName}|${msg.time}】${msg.ext.user.nickName}:`, msg?.attach?.url));
    }
    else if (msg.type == "audio") {
        log.value.push(new PocketMessage().addVoice(`【${msg.channelName}|${msg.time}】${msg.ext.user.nickName}:`, msg?.attach?.url));
    }
    else if (msg.type == "custom") {
        log.value.push(new PocketMessage().add(`【${msg.channelName}|${msg.time}】${msg.ext.user.nickName}:发送了一条特殊消息！`));
    }
};

const liveMsg = function (t: any, event: Array<LiveRoomMessage>) {
    event.forEach(item => {
        if (wsReady.value) {
            item.fromType = 2;
            ws.value?.send(JSON.stringify(item));
        }
    })
}

const handleRoomSocketDisconnect = function (...context: any): void {
    log.value.push(new PocketMessage().add("登录连接状态已断开。"));
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
    if (liveNim.value) {
        liveNim.value.disconnect();
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
    if (liveNim.value) {
        liveNim.value.disconnect();
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
    windStatus.value=res.data.windStatus;
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
            message: '删除成功！',
            type: 'success'
        });
        pic.value.splice(index, 1);
    }
    if (!res.data) {
        ElMessage({
            showClose: true,
            message: '删除失败！',
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

const saveBlogByid = () => {
    ElMessageBox.prompt('', '请输入微博id', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        beforeClose: (action: string, instance: any, done: Function) => {
            if (action == "confirm") {
                const value = instance.inputValue;
                if (!value || value.trim() === '') {
                    ElMessage({
                        showClose: true,
                        message: "请输入微博id",
                        type: 'warning'
                    });
                } else {
                    done();
                }
            } else {
                done();
            }
        }
    })
        .then(async ({ value }: { value: string }) => {
            if (!value || value.trim() === '') return;
            var res = await axios({
                url: "http://parkerbot.api/api/SaveByBlogId?blogId=" + value
            });
            if (res.data.success) {
                ElMessage({
                    showClose: true,
                    message: res.data.msg || '抓取成功！',
                    type: 'success'
                })
            } else {
                ElMessage({
                    showClose: true,
                    message: res.data.msg || "抓取失败！",
                    type: 'error'
                });
            }
        })
}
const windControl = () => {
    windStatus.value = !windStatus.value
    ElMessage.success(windStatus.value ? "已开启风控模式" : "已关闭风控模式")
    axios.get('http://parkerbot.api/api/SetWindStatus', { params: { windStatus: windStatus.value } })
}
</script>