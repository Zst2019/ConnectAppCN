using System.Collections.Generic;
using ConnectApp.Api;
using ConnectApp.Components;
using ConnectApp.Constants;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using ConnectApp.Plugins;
using ConnectApp.screens;
using ConnectApp.Utils;
using Unity.UIWidgets.Redux;

namespace ConnectApp.redux.actions {
    public class LoginChangeEmailAction : BaseAction {
        public string changeText;
    }

    public class LoginChangePasswordAction : BaseAction {
        public string changeText;
    }

    public class StartLoginByEmailAction : RequestAction {
    }

    public class LoginByEmailSuccessAction : BaseAction {
        public LoginInfo loginInfo;
    }

    public class LoginByEmailFailureAction : BaseAction {
    }

    public class LoginByWechatSuccessAction : BaseAction {
        public LoginInfo loginInfo;
    }

    public class LoginByWechatFailureAction : BaseAction {
    }

    public class LogoutAction : BaseAction {
    }

    public class CleanEmailAndPasswordAction : BaseAction {
    }

    public static partial class Actions {
        public static object loginByEmail() {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                var email = getState().loginState.email;
                var password = getState().loginState.password;
                return LoginApi.LoginByEmail(email, password)
                    .Then(loginInfo => {
                        var user = new User {
                            id = loginInfo.userId,
                            fullName = loginInfo.userFullName,
                            avatar = loginInfo.userAvatar,
                            title = loginInfo.title,
                            coverImage = loginInfo.coverImageWithCDN
                        };
                        var dict = new Dictionary<string, User> {
                            {user.id, user}
                        };
                        dispatcher.dispatch(new UserMapAction {userMap = dict});
                        dispatcher.dispatch(new LoginByEmailSuccessAction {
                            loginInfo = loginInfo
                        });
                        dispatcher.dispatch(new MainNavigatorPopAction());
                        dispatcher.dispatch(new CleanEmailAndPasswordAction());
                        UserInfoManager.saveUserInfo(loginInfo);
                        AnalyticsManager.LoginEvent("email");
                        AnalyticsManager.AnalyticsLogin("email", loginInfo.userId);
                        JPushPlugin.setJPushAlias(loginInfo.userId);
                        EventBus.publish(sName: EventBusConstant.login_success, new List<object>());
                    })
                    .Catch(error => {
                        dispatcher.dispatch(new LoginByEmailFailureAction());
                        var customSnackBar = new CustomSnackBar(
                            "邮箱或密码不正确，请稍后再试。"
                        );
                        customSnackBar.show();
                    });
            });
        }

        public static object loginByWechat(string code) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return LoginApi.LoginByWechat(code: code)
                    .Then(loginInfo => {
                        CustomDialogUtils.hiddenCustomDialog();
                        var user = new User {
                            id = loginInfo.userId,
                            fullName = loginInfo.userFullName,
                            avatar = loginInfo.userAvatar,
                            title = loginInfo.title,
                            coverImage = loginInfo.coverImageWithCDN
                        };
                        var dict = new Dictionary<string, User> {
                            {user.id, user}
                        };
                        dispatcher.dispatch(new UserMapAction {userMap = dict});
                        dispatcher.dispatch(new LoginByWechatSuccessAction {
                            loginInfo = loginInfo
                        });
                        UserInfoManager.saveUserInfo(loginInfo);
                        AnalyticsManager.LoginEvent("wechat");
                        AnalyticsManager.AnalyticsLogin("wechat", loginInfo.userId);
                        JPushPlugin.setJPushAlias(loginInfo.userId);
                        if (loginInfo.anonymous) {
                            LoginScreen.navigator.pushReplacementNamed(routeName: LoginNavigatorRoutes
                                .WechatBindUnity);
                        }
                        else {
                            dispatcher.dispatch(new MainNavigatorPopAction());
                            EventBus.publish(sName: EventBusConstant.login_success, new List<object>());
                        }
                    })
                    .Catch(error => {
                        CustomDialogUtils.hiddenCustomDialog();
                        dispatcher.dispatch(new LoginByWechatFailureAction());
                    });
            });
        }

        public static object loginByQr(string token, string action) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return LoginApi.LoginByQr(token: token, action: action)
                    .Then(success => {
                        if (action != "confirm") {
                            return;
                        }
                        CustomDialogUtils.hiddenCustomDialog();
                        dispatcher.dispatch(new MainNavigatorPopAction());
                        CustomDialogUtils.showToast(
                            success ? "扫码成功" : "扫码失败",
                            success ? Icons.sentiment_satisfied : Icons.sentiment_dissatisfied
                        );
                    })
                    .Catch(error => {
                        if (action != "confirm") {
                            return;
                        }
                        CustomDialogUtils.hiddenCustomDialog();
                        dispatcher.dispatch(new MainNavigatorPopAction());
                        CustomDialogUtils.showToast("扫码失败", iconData: Icons.sentiment_dissatisfied);
                    });
            });
        }

        public static object openCreateUnityIdUrl() {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return LoginApi.FetchCreateUnityIdUrl()
                    .Then(url => { dispatcher.dispatch(new OpenUrlAction {url = url}); });
            });
        }
    }
}