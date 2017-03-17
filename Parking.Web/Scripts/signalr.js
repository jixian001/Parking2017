//signalr 引入
$(function () {
    var hubs = $.connection.parkingHub;

    //供服务端调用的方法定义
    $.extend(hubs.client, {
        serveOpened: function () {
            $("#svcStatus").html("启动中...");
            $("#svcRun").removeClass("btn-info").addClass("btn-default").prop("disabled", true);
            $("#svcStop").addClass("btn-info").prop("disabled", false);
        },

        serveClosed: function () {
            $("#svcStatus").html("已停止...");
            $("#svcRun").addClass("btn-info").prop("disabled", false);
            $("#svcStop").removeClass("btn-info").addClass("btn-default").prop("disabled", true);
        },
        //Define a function to get 'location updated' events
        //lctn为json对象
        feedbackLocInfo: function (lctn) {
            console.log(lctn);

        }
    });

    //添加客户端调用
    $("#svcRun").click(function () {
        checkLogin("run");
    });

    $("#svcStop").click(function () {
        BootstrapDialog.show({
            title: "友情提示",
            message: "<p class='text-danger text-center'>你确定要关闭后台服务？ <span class='text-info'>关闭后将无法对设备进行自动控制！</span></p>",
            buttons: [{
                label: "确 定",
                action: function (self) {
                    checkLogin("stop");
                    self.close();
                }
            }, {
                label: "取 消",
                action: function (self) {
                    self.close();
                }
            }]
        });
    });
   
    //检查客户端
    function checkLogin(name) {
        $.ajax({
            url: "/Account/Validate",
            type: "GET",
            cache: false,
            dataType: "text",
            success: function (data, state) {

                if (data == "true") {
                    if (name == "run") {
                        hubs.server.openServe();
                    }
                    else if (name == "stop") {
                        hubs.server.closeServe();
                    }
                }
                else {
                    BootstrapDialog.show({
                        title: "温馨提示",
                        message: "<p class='text-info text-center'>请登陆后，再执行操作！</p>",
                        buttons: [{
                            label: "关闭",
                            action: function (self) {
                                self.close();
                            }
                        }]
                    });
                }
            },
            error: function (er) {
                console.log(er);
            }
        });
    }

    //start the connection
    $.connection.hub.start()
                    .then(function () {
                        return hubs.server.getStatus();
                    })
                    .done(function (status) {
                        if (status == "Close") {
                            hubs.client.serveClosed();
                        } else if (status == "Open") {
                            hubs.client.serveOpened();
                        }
                    });

});