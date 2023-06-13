from flask import Flask        # 导入Flask类
from aligo import Aligo
from flask import request

app = Flask(__name__)          # 实例化Flask类
    
@app.route('/ali/upload') 
def UploadImgToAlbumOf66():
    try:
        path = request.args['path']
        album = request.args['album']
        CreateAlbum(album)
        fileId = CreateFolder(album)
        fileInfo = ali.upload_file(file_path=path, parent_file_id=fileId)
        for albumInfo in ali.list_albums():
            if albumInfo.name==album:
                ali.add_files_to_album(album_id=albumInfo.album_id, files=[fileInfo])
                return ApiResult.Success("上传云盘成功");
    except OSError as reason:
        return ApiResult.Faild(str(reason));

@app.route('/ali/getalbumphotos')
def GetAlbumPhotos():
    album = request.args['album']
    try:
        for albumInfo in ali.list_albums():
            if albumInfo.name==album:
                list = ali.list_album_files(album_id=albumInfo.album_id)
                print(ApiResult.DataResult(list))
                return ApiResult.DataResult(list)
        return ApiResult.DataResult('')
    except OSError as reason:
        return ApiResult.Faild(str(reason));

def CreateAlbum(album):
    list = ali.list_albums()
    albums = [x for x in list if x.name==album]
    if len(albums) == 0:
        ali.create_album(album)
def CreateFolder(folder):
    list = ali.get_file_list()
    folders = [x for x in list if x.name==folder]
    if len(folders) == 0:
        newFolder = ali.create_folder(folder)
        return newFolder.file_id
    elif len(folders) == 1:
        return folders[0].file_id
    return "root"

class ApiResult:
    code:int
    data:object
    msg:str
    def __init__(self) -> None:
        pass
    def Success(msg:str):
     api = ApiResult()
     api.code=0
     api.data=''
     api.msg=msg
     return JsonParse(api)
    def Faild(msg:str):
     api = ApiResult()
     api.code=500
     api.data=''
     api.msg=msg
     return JsonParse(api)
    def DataResult(data:object):
     api = ApiResult()
     api.code=0
     api.data=data
     api.msg=''
     return JsonParse(api)

def JsonParse(obj:ApiResult):
    return{
        'code':obj.code,
        'msg':obj.msg,
        'data':obj.data
    }

if __name__ == '__main__':     # 启动服务  
    ali = Aligo()
    app.run(host='127.0.0.1',port=5555)