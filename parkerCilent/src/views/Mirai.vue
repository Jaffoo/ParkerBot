<template>
    <el-container>
        <el-header>
            <el-button type="primary" native-type="button" :icon="ArrowLeft" @click="back">返回</el-button>
            <el-button type="primary" native-type="button" :icon="Edit" @click="save(miraiForm)">保存</el-button>
        </el-header>
        <el-main>
            <el-form ref="miraiForm" :model="mirai" label-width="120px">
                <el-form-item label="启用机器人" prop="useMirai">
                    <el-switch v-model="mirai.useMirai" :active-value="true" :inactive-value="false"></el-switch>
                </el-form-item>
                <el-form-item label="qq" prop="qq">
                    <el-input :disabled="!mirai.useMirai" v-model="mirai.qq"></el-input>
                </el-form-item>
                <el-form-item label="mirai根目录" prop="path">
                    <el-input :disabled="!mirai.useMirai" v-model="mirai.path" style="width:87%"></el-input>
                    <el-button :disabled="!mirai.useMirai" @click="getMiraiConfig" native-type="button"
                        type="primary">获取配置</el-button>
                </el-form-item>
                <el-form-item label="Tips：">
                    <span>配置正确目录后下方内容可以通过上方【获取配置】按钮获取。</span>
                </el-form-item>
                <el-form-item label="地址" prop="address">
                    <el-input :disabled="!mirai.useMirai" v-model="mirai.address"></el-input>
                </el-form-item>
                <el-form-item label="verifyKey" prop="verifyKey">
                    <el-input :disabled="!mirai.useMirai" v-model="mirai.verifyKey"></el-input>
                </el-form-item>
            </el-form>
        </el-main>
        <el-footer>
            <div>具体配置教学由于网上很多此处不再赘述，下面提供几个教学地址：</div>
            <div>1、GitHub：<span
                    style="color:rgb(19, 1, 173);text-decoration-line: underline;">https://sinoahpx.github.io/Mirai.Net.Documents/#/?id=mirainet-文档</span>
                通常来说你只用看到开始使用-快速开始-安装mirai环境即可，如果想了解什么是mirai可以看开始使用-基础知识-mirai生态
            </div>
            <div>2、CSDN博客：<span
                    style="color:rgb(19, 1, 173);text-decoration-line: underline;">https://blog.csdn.net/qq_30141543/article/details/125576220</span>
            </div>
            <div>3、哔哩哔哩：<span
                    style="color:rgb(19, 1, 173);text-decoration-line: underline;">https://www.bilibili.com/video/BV1eb4y1k7uX/</span>
            </div>
        </el-footer>
    </el-container>
</template>
<script setup lang="ts">
import 'element-plus/es/components/message/style/css'
import { ref, onMounted } from 'vue'
import axios from 'axios'
import { ArrowLeft, Edit } from '@element-plus/icons-vue'
import { type FormInstance, ElMessage } from 'element-plus'

const miraiForm = ref<FormInstance>();
const mirai = ref({
    useMirai: false,
    path: '',
    address: '',
    qq: '',
    verifyKey: ''
})
onMounted(() => {
    axios({
        url: "http://parkerbot.api/api/GetMiraiConfig"
    }).then(res => {
        mirai.value = res.data;
    })
})
const getMiraiConfig = function () {
    axios({
        url: "http://parkerbot.api/api/getMiraiFile",
    }).then(res => {
        if (res.data == false) {
            ElMessage({
                showClose: true,
                message: '目录不存在或路径错误，请检查后重试！',
                type: 'error'
            });
        }
        else {
            mirai.value.address = res.data.address;
            mirai.value.verifyKey = res.data.verifyKey;
            ElMessage({
                showClose: true,
                message: '获取成功！',
                type: 'success'
            });
        }
    })
}

const back = () => {
    history.go(-1);
}
//表单提交
const save = async (formEl: FormInstance | undefined) => {
    if (!formEl) return;
    await formEl.validate((valid, fields) => {
        if (valid) {
            axios({
                url: "http://parkerbot.api/api/setMiraiconfig",
                method: 'post',
                data: mirai.value
            }).then(res => {
                if (res.data) {
                    ElMessage({
                        showClose: true,
                        message: '保存成功',
                        type: 'success'
                    });
                } else {
                    ElMessage({ message: "修改失败，请重试。如果连续失败，请联系作者！", type: 'error' });
                    return false;
                }
            });
        }
    });
}

</script>