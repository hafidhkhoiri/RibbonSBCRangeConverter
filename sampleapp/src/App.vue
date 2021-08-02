<template>
  <v-app>
  <v-navigation-drawer app>
    <!-- -->
  </v-navigation-drawer>

  <v-app-bar app>
    <!-- -->
  </v-app-bar>

  <!-- Sizes your content based upon application components -->
  <v-main>

    <!-- Provides the application the proper gutter -->
    <v-container fluid>
      <div style="height:400px;width:600px;position:relative">
      </div>
      <!-- If using vue-router -->
      <router-view></router-view>
    </v-container>
  </v-main>

  <v-footer app>
    <!-- -->
  </v-footer>
</v-app>
</template>

<script>

export default {
  name: 'App',
};
</script>
