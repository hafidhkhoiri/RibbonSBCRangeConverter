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
      <v-row
        align="center"
      >
        <v-col cols="12">
          <v-autocomplete
            clearable
            v-model="customers.value"
            :items="customers.items"
            label="Pick a customer"
          ></v-autocomplete>
        </v-col>
      </v-row>

      <v-card>
        <v-tabs
          v-model="tab"
          background-color="primary"
          dark
        >
          <v-tab
            v-for="item in items"
            :key="item.tab"
          >
            {{ item.tab }}
          </v-tab>
        </v-tabs>

        <v-tabs-items v-model="tab">
           <v-tab-item key="Summary">
             <v-container fluid>
                <v-row>
                  <v-col
                    cols="12"
                    md="12"
                  >
                    <v-card flat>
                      <Hellox></Hellox>
                    </v-card>
                  </v-col>
                </v-row>
             </v-container>
          </v-tab-item>
          <v-tab-item key="Numbers">
              <NumbersList />
          </v-tab-item>

           <v-tab-item key="Ranges">
             <NumberRange />
          </v-tab-item>
        </v-tabs-items>
      </v-card>

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
import Hellox from './components/HelloWorld.vue';
import NumbersList from './components/NumbersList.vue';
import NumberRange from './components/NumberRange.vue';

export default {
  name: 'App',
  components:{
    Hellox,
    NumbersList,
    NumberRange
  },
  data: () => ({
    customers:{
      items: ['Errai Pasifik', 'Telkom'],
      values: ['Errai Pasifik', 'Telkom'],
      value: null,
    },
    tab: null,
    items: [
      { tab: 'Summary', content: 'Tab 1 Content' },
      { tab: 'Numbers', content: 'Tab 1 Content' },
      { tab: 'Ranges', content: 'Tab 2 Content' },
    ],
    
  }),
};
</script>