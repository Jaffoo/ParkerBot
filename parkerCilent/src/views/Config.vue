<template>
    <el-container>
        <el-header>
            <el-button type="primary" native-type="button" :icon="ArrowLeft" @click="back">返回</el-button>
            <el-button type="primary" native-type="button" :icon="Edit" @click="save(form)">保存</el-button>
        </el-header>
        <el-main>
            <div style="margin-bottom:1%">
                <label style="font-size:18px;margin-right:3%">启用模块</label>
                <el-checkbox v-model="eable.qq" label="QQ"></el-checkbox>
                <el-checkbox v-model="eable.wb" label="微博"></el-checkbox>
                <el-checkbox v-model="eable.bz" label="B站"></el-checkbox>
                <el-checkbox v-model="eable.kd" label="口袋48"></el-checkbox>
                <el-checkbox v-model="eable.xhs" label="小红书"></el-checkbox>
                <el-checkbox v-model="eable.dy" label="抖音"></el-checkbox>
                <el-checkbox v-model="eable.bd" label="百度"></el-checkbox>
            </div>
            <el-collapse>
                <el-form ref="form" :model="config" label-width="100px">
                    <el-collapse-item title="QQ" v-if="eable.qq" name="qq">
                        <el-form-item label="QQ群" prop="QQ.group" :rules="rules.input">
                            <el-input v-model="config.QQ.group"
                                placeholder="多个用英文逗号,分隔；不填写则qq机器人所在的全部群开启，填写则针对填写的群开启"></el-input>
                        </el-form-item>
                        <el-form-item label="超级管理员" prop="QQ.admin" :rules="rules.input">
                            <el-input v-model="config.QQ.admin" placeholder="仅支持配置一个账号"></el-input>
                        </el-form-item>
                        <el-form-item label="管理员">
                            <el-input v-model="config.QQ.permission" placeholder="多个用英文逗号,分隔"></el-input>
                        </el-form-item>
                        <el-form-item label="启用功能">
                            <el-checkbox-group v-model="funcsChecked">
                                <el-checkbox v-for="item in funcs" :label="item.value" :key="item.id">{{ item.name
                                }}</el-checkbox>
                            </el-checkbox-group>
                        </el-form-item>
                        <el-form-item label="功能分类">
                            <el-transfer v-model="config.QQ.funcUser1" :data="config.QQ.funcEnable1"
                                :titles="['管理员可用', '普通用户可用']" :left-default-checked=config.QQ.funcAdmin1
                                :right-default-checked=config.QQ.funcUser1 :props="{ key: 'value', label: 'name' }">
                            </el-transfer>
                        </el-form-item>
                        <el-form-item label="敏感词">
                            <el-input type="textarea" v-model="config.QQ.sensitive" placeholder="多个用英文逗号,分隔"></el-input>
                        </el-form-item>
                        <el-form-item label="敏感词操作">
                            <el-select v-model="config.QQ.actions" multiple placeholder="请选择" collapse-tags
                                style="width:30%">
                                <el-option label="群内提示" value="1"></el-option>
                                <el-option label="私信群管理员" value="2"></el-option>
                                <el-option label="私信机器人管理员" value="3"></el-option>
                                <el-option label="私信机器人超管" value="4"></el-option>
                                <el-option label="撤回" value="5"></el-option>
                            </el-select>
                            <span style="color:red">*撤回机器人需要管理员权限，私信需要与机器人为好友</span>
                        </el-form-item>
                    </el-collapse-item>
                    <el-collapse-item title="微博" v-if="eable.wb" name="wb">
                        <el-form-item label="微博地址" prop="WB.url" :rules="rules.input">
                            <el-input type="textarea" v-model="config.WB.url" placeholder="多个用回车键分隔"></el-input>
                        </el-form-item>
                        <el-form-item label="监听间隔" prop="WB.timeSpan" :rules="rules.input">
                            <el-input type="number" v-model="config.WB.timeSpan" placeholder="单位分钟"></el-input>
                        </el-form-item>
                        <el-form-item label="转发至qq群">
                            <el-switch v-model="config.WB.forwardGroup" :active-value="true"
                                :inactive-value="false"></el-switch>
                        </el-form-item>
                        <el-form-item label="qq群" v-show="config.WB.forwardGroup === true">
                            <el-input v-model="config.WB.group" placeholder="发新微博转发消息"></el-input>
                            <span style="color:red">*多个用英文逗号,分隔；不填写则使用qq配置的群</span>
                        </el-form-item>
                        <el-form-item label="转发至qq好友">
                            <el-switch v-model="config.WB.forwardQQ" :active-value="true"
                                :inactive-value="false"></el-switch>
                        </el-form-item>
                        <el-form-item label="qq好友" v-show="config.WB.forwardQQ === true">
                            <el-input v-model="config.WB.qq" placeholder="发新微博转发消息"></el-input>
                            <span style="color:red">*多个用英文逗号,分隔；不填写则默认超管</span>
                        </el-form-item>
                    </el-collapse-item>
                    <el-collapse-item title="B站" v-if="eable.bz" name="bz">
                        <el-form-item label="B站地址" prop="BZ.url" :rules="rules.input">
                            <el-input type="textarea" v-model="config.BZ.url" placeholder="多个用回车键分隔"></el-input>
                        </el-form-item>
                        <el-form-item label="监听间隔" prop="BZ.timeSpan" :rules="rules.input">
                            <el-input type="number" v-model="config.BZ.timeSpan" placeholder="单位分钟"></el-input>
                        </el-form-item>
                        <el-form-item label="转发至qq群">
                            <el-switch v-model="config.BZ.forwardGroup" :active-value="true"
                                :inactive-value="false"></el-switch>
                        </el-form-item>
                        <el-form-item label="qq群" v-show="config.BZ.forwardGroup === true">
                            <el-input v-model="config.BZ.group" placeholder="发新动态转发消息"></el-input>
                            <span style="color:red">*多个用英文逗号,分隔；不填写则使用qq配置的群</span>
                        </el-form-item>
                        <el-form-item label="转发至qq好友">
                            <el-switch v-model="config.BZ.forwardQQ" :active-value="true"
                                :inactive-value="false"></el-switch>
                        </el-form-item>
                        <el-form-item label="qq好友" v-show="config.BZ.forwardQQ === true">
                            <el-input v-model="config.BZ.qq" placeholder="发新微博转发消息"></el-input>
                            <span style="color:red">*多个用英文逗号,分隔；不填写则默认超管</span>
                        </el-form-item>
                    </el-collapse-item>
                    <el-collapse-item title="口袋48" v-if="eable.kd" name="kd">
                        <el-form-item label="IMServerId" prop="KD.serverId" :rules="rules.input">
                            <el-input v-model="config.KD.serverId"></el-input>
                        </el-form-item>
                        <el-form-item label="IM账号" prop="KD.account" :rules="rules.input">
                            <el-input v-model="config.KD.account"></el-input>
                        </el-form-item>
                        <el-form-item label="IMtoken" prop="KD.token" :rules="rules.input">
                            <el-input v-model="config.KD.token"></el-input>
                        </el-form-item>
                        <el-form-item>
                            <el-button @click="loginKD = true">登录口袋48</el-button>
                            <span style="color:red">*以上信息不知道可点此登录口袋自动获取</span>
                        </el-form-item>
                        <el-form-item label="转发至qq群">
                            <el-switch v-model="config.KD.forwardGroup" :active-value="true"
                                :inactive-value="false"></el-switch>
                        </el-form-item>
                        <el-form-item label="qq群" v-show="config.KD.forwardGroup === true">
                            <el-input v-model="config.KD.group" placeholder="发新消息转发消息"></el-input>
                            <span style="color:red">*多个用英文逗号,分隔；不填写则使用qq配置的群</span>
                        </el-form-item>
                        <el-form-item label="转发至qq好友">
                            <el-switch v-model="config.KD.forwardQQ" :active-value="true"
                                :inactive-value="false"></el-switch>
                        </el-form-item>
                        <el-form-item label="qq好友" v-show="config.KD.forwardQQ === true">
                            <el-input v-model="config.KD.qq" placeholder="发新微博转发消息"></el-input>
                            <span style="color:red">*多个用英文逗号,分隔；不填写则默认超管</span>
                        </el-form-item>
                    </el-collapse-item>
                    <el-collapse-item title="小红书" v-if="eable.xhs" name="xhs">
                        <el-form-item label="小红书地址" prop="XHS.url" :rules="rules.input">
                            <el-input type="textarea" v-model="config.XHS.url" placeholder="多个用回车键分隔"></el-input>
                        </el-form-item>
                        <el-form-item label="监听时长" prop="XHS.timeSpan" :rules="rules.input">
                            <el-input type="number" v-model="config.XHS.timeSpan" placeholder="单位分钟"></el-input>
                        </el-form-item>
                        <el-form-item label="转发至qq群">
                            <el-switch v-model="config.XHS.forwardGroup" :active-value="true"
                                :inactive-value="false"></el-switch>
                        </el-form-item>
                        <el-form-item label="qq群" v-show="config.XHS.forwardGroup === true">
                            <el-input v-model="config.XHS.group" placeholder="发新动态转发消息"></el-input>
                            <span style="color:red">*多个用英文逗号,分隔；不填写则使用qq配置的群</span>
                        </el-form-item>
                        <el-form-item label="转发至qq好友">
                            <el-switch v-model="config.XHS.forwardQQ" :active-value="true"
                                :inactive-value="false"></el-switch>
                        </el-form-item>
                        <el-form-item label="qq好友" v-show="config.XHS.forwardQQ === true">
                            <el-input v-model="config.XHS.qq" placeholder="发新微博转发消息"></el-input>
                            <span style="color:red">*多个用英文逗号,分隔；不填写则默认超管</span>
                        </el-form-item>
                    </el-collapse-item>
                    <el-collapse-item title="抖音" v-if="eable.dy" name="dy">
                        <el-form-item label="抖音地址" prop="DY.url" :rules="rules.input">
                            <el-input type="textarea" v-model="config.DY.url" placeholder="多个用回车键分隔"></el-input>
                        </el-form-item>
                        <el-form-item label="监听时长" prop="DY.timeSpan" :rules="rules.input">
                            <el-input type="number" v-model="config.DY.timeSpan" placeholder="单位分钟"></el-input>
                        </el-form-item>
                        <el-form-item label="转发至qq群">
                            <el-switch v-model="config.DY.forwardGroup" :active-value="true"
                                :inactive-value="false"></el-switch>
                        </el-form-item>
                        <el-form-item label="qq群" v-show="config.DY.forwardGroup === true">
                            <el-input v-model="config.DY.group" placeholder="发新抖音转发消息"></el-input>
                            <span style="color:red">*多个用英文逗号,分隔；不填写则使用qq配置的群</span>
                        </el-form-item>
                        <el-form-item label="转发至qq好友">
                            <el-switch v-model="config.DY.forwardQQ" :active-value="true"
                                :inactive-value="false"></el-switch>
                        </el-form-item>
                        <el-form-item label="qq好友" v-show="config.DY.forwardQQ === true">
                            <el-input v-model="config.DY.qq" placeholder="发新微博转发消息"></el-input>
                            <span style="color:red">*多个用英文逗号,分隔；不填写则默认超管</span>
                        </el-form-item>
                    </el-collapse-item>
                    <el-collapse-item title="百度" v-if="eable.bd" name="bd">
                        <el-form-item label="appKey" prop="BD.appKey" :rules="rules.input">
                            <el-input v-model="config.BD.appKey" placeholder="百度appKey"></el-input>
                        </el-form-item>
                        <el-form-item label="appSeret" prop="BD.appSeret" :rules="rules.input">
                            <el-input v-model="config.BD.appSeret" placeholder="百度appSeret"></el-input>
                        </el-form-item>
                        <el-form-item label="开启人脸验证">
                            <el-switch v-model="config.BD.faceVerify" :active-value="true"
                                :inactive-value="false"></el-switch>
                        </el-form-item>
                        <el-form-item label="基础人脸" v-show="config.BD.faceVerify" prop="BD.imageList1"
                            :rules="rules.imageList1">
                            <el-upload :file-list="config.BD.imageList1" action="http://parkerbot.api/api/upload"
                                :on-success="onSuccess" :on-remove="onRemove" list-type="picture-card" :limit="3"
                                accept=".jpg,.png,.jpeg">
                                <el-icon>
                                    <Plus />
                                </el-icon>
                            </el-upload>
                            <span style="color:red">*上传人脸轮廓清晰的图片</span>
                        </el-form-item>
                        <el-form-item label="人脸相似度" v-show="config.BD.faceVerify" prop="BD.similarity" :rules="rules.audit">
                            <el-col :span="8">
                                <el-input type="number" v-model="config.BD.similarity"></el-input>
                            </el-col>
                            <span style="color:red">*直接保存(非双胞胎建议80，双胞胎建议70)</span>
                        </el-form-item>
                        <el-form-item label="审核相似度" v-show="config.BD.faceVerify" prop="BD.audit" :rules="rules.audit">
                            <el-col :span="8">
                                <el-input type="number" v-model="config.BD.audit"></el-input>
                            </el-col>
                            <span style="color:red">*超过该值，但未超过上面的值，将加入审核列表，审核通过才会保存</span>
                        </el-form-item>
                        <el-form-item label="上传云盘" v-show="config.BD.faceVerify">
                            <el-switch v-model="config.BD.saveAliyunDisk" :active-value="true"
                                :inactive-value="false"></el-switch>
                            <span style="color:red">*启用会将图片自动上传到阿里云盘相册</span>
                        </el-form-item>
                        <el-form-item label="相册名称" v-show="config.BD.saveAliyunDisk" prop="BD.albumName"
                            :rules="rules.saveAliyunDisk">
                            <el-col :span="8">
                                <el-input v-model="config.BD.albumName"></el-input>
                            </el-col>
                        </el-form-item>
                    </el-collapse-item>
                </el-form>
            </el-collapse>
        </el-main>
    </el-container>
    <el-dialog title="登录口袋48" v-model="loginKD" :before-close="close" :close-on-click-modal="false">
        <el-form label-width="100px">
            <el-form-item label="手机号" required class="mt-4">
                <el-input v-model="config.KD.phone" style="width:95%">
                    <template #prepend>+{{ config.KD.area }}</template>
                </el-input>
            </el-form-item>
            <el-form-item label="验证码" required>
                <el-input type="primary" v-model="config.KD.code" style="width:65%"></el-input><el-button
                    v-show="!config.KD.hasSend" style="width:25%;margin-left:5%" @click="send">发送验证码</el-button><el-button
                    v-show="config.KD.hasSend" style="width:25%;margin-left:5%">{{ config.KD.sec }}秒</el-button>
            </el-form-item>
            <el-form-item>
                <el-button type="primary" @click="submitForm">登录</el-button>
            </el-form-item>
        </el-form>
    </el-dialog>
</template>
<script setup lang="ts">
import 'element-plus/es/components/message/style/css'
import { ref, watch, onMounted } from 'vue'
import axios from 'axios'
import { type FormInstance, ElMessage } from 'element-plus'
import { ArrowLeft, Edit, Plus } from '@element-plus/icons-vue'
import type { UploadProps, UploadUserFile } from 'element-plus'

const funcs = ref([] as any[]);
const funcsChecked = ref([] as any[]);
const loginKD = ref(false);
const eable = ref({
    qq: false,
    wb: false,
    bz: false,
    kd: false,
    xhs: false,
    dy: false,
    bd: false,
});
const form = ref<FormInstance>();
const config = ref({
    QQ: {
        group: '',
        funcEnable: "",
        funcEnable1: [] as any[],
        funcAdmin: "",
        funcAdmin1: [] as any[],
        funcUser: "",
        funcUser1: [] as any[],
        admin: '',
        permission: '',
        sensitive: "",
        action: '',
        actions: Array<string>()
    },
    WB: {
        url: '',
        timeSpan: 0,
        group: '',
        forwardGroup: false,
        qq: '',
        forwardQQ: false,
    },
    BZ: {
        url: '',
        timeSpan: 0,
        group: '',
        forwardGroup: false,
        qq: '',
        forwardQQ: false,
    },
    KD: {
        sec: 60,
        hasSend: false,
        area: '86',
        code: '',
        phone: '',
        group: '',
        forwardGroup: false,
        token: '',
        account: '',
        serverId: '',
        qq: '',
        forwardQQ: false,
    },
    XHS: {
        url: '',
        timeSpan: 0,
        group: '',
        forwardGroup: false,
        qq: '',
        forwardQQ: false,
    },
    DY: {
        url: '',
        timeSpan: 0,
        group: '',
        forwardGroup: false,
        qq: '',
        forwardQQ: false,
    },
    BD: {
        appKey: '',
        appSeret: '',
        faceVerify: false,
        imageList: "",
        imageList1: new Array<UploadUserFile>(),
        similarity: 0,
        saveAliyunDisk: false,
        audit: 0,
        albumName: ''
    }
})
const rules = ref({
    input: [{ required: true, message: "请输入该值", trigger: "blur" }],
    imageList1: [{
        required: true, validator: (rule: any, value: any, callback: any) => {
            if (config.value.BD.faceVerify && config.value.BD.imageList1.length <= 0) {
                callback(new Error('请上传图片'))
            }
            callback();
        },
        trigger: ['blur', 'change']
    }],
    audit: [{
        required: true, validator: (rule: any, value: any, callback: any) => {
            if (config.value.BD.faceVerify && !value) {
                callback(new Error('请输入该值'))
            }
            callback()
        },
        trigger: ['blur', 'change']
    }],
    saveAliyunDisk: [{
        required: true, validator: (rule: any, value: any, callback: any) => {
            if (config.value.BD.saveAliyunDisk && !value) {
                callback(new Error('请输入该值'))
            }
            callback()
        },
        trigger: ['blur', 'change']
    }],
})
watch(funcsChecked.value, (newVal, oldVal) => {
    if (newVal.length <= 0) {
        config.value.QQ.funcEnable1 = [];
        return;
    }
    if (oldVal.length <= 0) {
        config.value.QQ.funcEnable1 = newVal;
        return;
    }
    var ids = newVal.filter(function (v) {
        return oldVal.indexOf(v) === -1;
    }).concat(oldVal.filter(function (v) { return newVal.indexOf(v) === -1; }));
    var id = ids[0];
    if (newVal.length > oldVal.length) {
        funcs.value.forEach((item: any) => {
            if (item.value == id) {
                if (!config.value.QQ.funcEnable1.some((item1: any) => { return item1.value == id; })) {
                    config.value.QQ.funcEnable1.push(item);
                    return;
                }
            }
        });
    }
    if (newVal.length < oldVal.length) {
        var index = -1;
        config.value.QQ.funcEnable1.forEach((item: any, i: number) => {
            if (item.value == id) {
                index = i;
                return;
            }
        });
        config.value.QQ.funcEnable1.splice(index, 1);
    }
})
onMounted(() => {
    axios({
        url: "http://parkerbot.api/api/GetBaseConfig"
    }).then(res => {
        for (let propName in eable.value) {
            for (let item of res.data.enable) {
                if (item.key.toLowerCase() == propName) {
                    if(item.value==="true"||item.value==="True"){
                        eable.value[propName] = true;
                    }else{
                        eable.value[propName] = false;
                    }
                    continue;
                }
            }
        }
        res.data.config.QQ.funcEnable1 = res.data.config.QQ.funcEnable1 ?? new Array();
        res.data.config.QQ.funcAdmin1 = res.data.config.QQ.funcAdmin1 ?? new Array();
        res.data.config.QQ.funcUser1 = res.data.config.QQ.funcUser1 ?? new Array();
        config.value = res.data.config;
        config.value.QQ.actions=config.value.QQ.action.split(",")
        config.value.KD.area = '86';
        funcsChecked.value = res.data.config.QQ.funcEnable1;
    });

    axios({
        url: 'http://parkerbot.api/api/GetQQFun'
    }).then(res => {
        funcs.value = res.data;
    });
})

const back = () => {
    history.go(-1);
}

//表单提交
const save = async (formEl: FormInstance | undefined) => {
    if (!formEl) return;
    await formEl.validate((valid, fields) => {
        config.value.BD.imageList=config.value.BD.imageList1.map(item=>{
            return item.url;
        }).toString();
        if (valid) {
            config.value.QQ.action=config.value.QQ.actions.toString();
            axios({
                url: "http://parkerbot.api/api/SetConfig",
                method: "post",
                data: { config: config.value, enable: eable.value }
            }).then(res => {
                if (res.data) {
                    ElMessage({ message: "修改成功", type: 'success' });
                } else {
                    ElMessage({ message: "修改失败，请重试。如果连续失败，请联系作者！", type: 'error' });
                }
            });
        } else {
            ElMessage({ message: "请检查配置信息是否均已填写完成", type: 'warning' });
            return false;
        }
    });
}

const close = () => {
    loginKD.value = false;
    config.value.KD.hasSend = false;
    config.value.KD.sec = 60;
}
const send = () => {
    if (!config.value.KD.phone) {
        ElMessage({ message: "请输入手机号码！", type: 'warning' });
        return;
    }
    var patrn = /^1[3456789]\d{9}$/;
    if (patrn.test(config.value.KD.phone) == false) {
        ElMessage({ message: "手机号码格式有误，请重新输入！", type: 'warning' });
        return;
    }
    config.value.KD.hasSend = true;
    subtraction();
    axios({
        url: 'https://pocketapi.48.cn/user/api/v1/sms/send2',
        method: 'post',
        headers: createHeader(""),
        responseType: 'json',
        timeout: 180_000,
        data: { mobile: config.value.KD.phone, area: config.value.KD.area }
    }).then(res => {
        if (res.data.success) {
            ElMessage({ message: "发送成功，请注意查收！", type: 'success' });
        }
    });
}
const submitForm = () => {
    if (!config.value.KD.phone) {
        ElMessage({ message: "请输入手机号码！", type: 'warning' });
        return;
    }
    if (!config.value.KD.code) {
        ElMessage({ message: "请输入验证码！", type: 'warning' });
        return;
    }
    var patrn = /^1[3456789]\d{9}$/;
    if (patrn.test(config.value.KD.phone) == false) {
        ElMessage({ message: "手机号码格式有误，请重新输入！", type: 'warning' });
        return;
    }
    axios({
        url: 'https://pocketapi.48.cn/user/api/v1/login/app/mobile/code',
        method: 'post',
        headers: createHeader(""),
        responseType: 'json',
        timeout: 180_000,
        data: { mobile: config.value.KD.phone, code: config.value.KD.code }
    }).then(res => {
        axios({
            url: 'https://pocketapi.48.cn/im/api/v1/im/userinfo',
            method: 'post',
            headers: createHeader(res.data.content.token),
            responseType: 'json',
            timeout: 180_000
        }).then(res1 => {
            config.value.KD.token = res1.data.content.pwd;
            config.value.KD.account = res1.data.content.accid;
        });
    });
}
const subtraction = () => {
    config.value.KD.sec = 60;
    let timer: number;
    timer = setInterval(() => {
        if (config.value.KD.sec > 0) {
            config.value.KD.sec--;
        } else {
            clearInterval(timer);
            config.value.KD.sec = 60;
            config.value.KD.hasSend = false;
        }
    }, 1000);
}
const uuid = function (): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0,
            v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}
const rStr = function (len: number): string {
    const str = 'QWERTYUIOPASDFGHJKLZXCVBNM1234567890';
    let result = '';
    for (let i = 0; i < len; i++) {
        const rIndex = Math.floor(Math.random() * str.length);
        result += str[rIndex];
    }
    return result;
}
const createHeader = function (token: string): object {
    var headers = {
        pa: uuid(),
        'Content-Type': 'application/json;charset=utf-8',
        appInfo: JSON.stringify({
            vendor: 'apple',
            deviceId: `${rStr(8)}-${rStr(4)}-${rStr(4)}-${rStr(4)}-${rStr(12)}`,
            appVersion: '6.2.2',
            appBuild: '21080401',
            osVersion: '11.4.1',
            osType: 'ios',
            deviceName: 'iPhone XR',
            os: 'ios'
        }),
        'Accept-Language': 'zh-Hans-AW;q=1',
        token: ''
    };
    if (token) {
        headers.token = token;
    };
    return headers;
}

const onSuccess: UploadProps['onSuccess'] = (response) => {
    config.value.BD.imageList1.push(response)
}

const onRemove: UploadProps['onRemove'] = (file) => {
    var i = -1;
    config.value.BD.imageList1.forEach((item, index) => {
        if (item.name === file.name) {
            i = index;
            return;
        }
    })
    if (i >= 0) {
        config.value.BD.imageList1.splice(i, 1);
    }
}
</script>