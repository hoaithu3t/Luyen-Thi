import DocumentEditQuestion from "app/components/admin/Document/document-edit-question/DocumentEditQuestion";

import { useAppContext } from "hooks/AppContext/AppContext";
import React, { Suspense, useEffect } from "react";
import { Route, Switch } from "react-router";

const AdminPage = React.lazy(() => import("./Admin/Admin"));
const AuthPage = React.lazy(() => import("./Auth/Auth"));
const DocumentPage = React.lazy(() => import("./Document/Document"));
const DocumentExam = React.lazy(
  () => import("./Document/document-exam/DocumentExam")
);
const HomePage = React.lazy(() => import("./Home/Home"));
const ProfilePage = React.lazy(() => import("./Profile/Profile"));
const NotFoundPage = React.lazy(() => import("./404/NotFound"));
interface RouterProps {
  path: string;
  component: React.FC;
  exact?: boolean;
  showHeader?: boolean;
  showFooter?: boolean;
}
// applayout
const routes: RouterProps[] = [
  {
    path: "/",
    component: HomePage,
    exact: true,
  },
  {
    path: "/home",
    component: HomePage,
  },
  {
    path: "/admin",
    component: AdminPage,
    showHeader: false,
    showFooter: false,
  },
  {
    path: "/profile",
    component: ProfilePage,
  },
  {
    path: "/document/:id",
    component: DocumentExam,
    exact: true,
    showHeader: false,
    showFooter: false,
  },
  {
    path: "/document",
    component: DocumentPage,
    exact: false,
  },

  {
    path: "/class-room",
    component: DocumentPage,
    exact: false,
  },
  {
    path: "/practice",
    component: DocumentPage,
    exact: false,
  },
  {
    path: "/404",
    component: NotFoundPage,
  },
  {
    path: "/editor/document/:id",
    component: DocumentEditQuestion,
    showHeader: false,
    showFooter: false,
  },
  {
    path: "/auth",
    component: AuthPage,
  },
];
const Routes: React.FC = () => {
  return (
    <Suspense fallback={<></>}>
      <Switch>
        {routes.map((route, i) => (
          <Route path={route.path} exact={route.exact} key={i}>
            <RouterComponent {...route} />
          </Route>
        ))}
        {/* <Redirect from="*" to="/404"></Redirect> */}
      </Switch>
    </Suspense>
  );
};
const RouterComponent: React.FC<RouterProps> = (props) => {
  const { showFooter = true, showHeader = true } = props;
  const { setShowFooter, setShowHeader } = useAppContext();
  useEffect(() => {
    setShowHeader(showHeader);
    setShowFooter(showFooter);
  });
  return <>{React.createElement(props.component)}</>;
};

export default Routes;
